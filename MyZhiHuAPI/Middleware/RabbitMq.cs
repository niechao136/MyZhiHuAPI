using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;
using Newtonsoft.Json;

namespace MyZhiHuAPI.Middleware;

public class RabbitMqMiddleware(RequestDelegate next, IOptions<RabbitMqOption> option)
{

    private readonly RabbitMqOption _option = option.Value;

    public async Task Invoke(HttpContext context)
    {
        var rabbitMq = context.GetEndpoint()!.Metadata.GetMetadata<RabbitMqAttribute>();
        if (rabbitMq == null) await next(context);
        else
        {
            var originalBody = context.Response.Body;
            using var stream = new MemoryStream();
            context.Response.Body = stream;
            using var original = new MemoryStream();
            await context.Request.Body.CopyToAsync(original);
            original.Position = 0;
            var request = await new StreamReader(original).ReadToEndAsync();
            original.Position = 0;
            context.Request.Body = original;

            await next(context);

            var needNotify = false;
            var notify = new Notify<object>();
            stream.Position = 0;
            var response = await new StreamReader(stream).ReadToEndAsync();
            stream.Position = 0;
            await stream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            switch (rabbitMq.Type)
            {
                case NotifyType.Answer:
                    var answer = JsonConvert.DeserializeObject<MessageModel<Answer>>(response);
                    if (answer!.Status == StatusCode.Success)
                    {
                        needNotify = true;
                        notify.Target = answer.Data;
                        notify.Type = NotifyType.Answer;
                        notify.Target_id = answer.Data!.Id;
                        notify.Opeate_id = answer.Data!.Owner_id;
                    }
                    break;
                case NotifyType.Commit:
                    var commit = JsonConvert.DeserializeObject<MessageModel<Commit>>(response);
                    if (commit!.Status == StatusCode.Success)
                    {
                        needNotify = true;
                        notify.Target = commit.Data;
                        notify.Type = NotifyType.Commit;
                        notify.Target_id = commit.Data!.Id;
                        notify.Opeate_id = commit.Data!.Owner_id;
                    }
                    break;
                case NotifyType.AnswerAgree:
                    var req = JsonConvert.DeserializeObject<AnswerAgree>(request);
                    var agree = JsonConvert.DeserializeObject<MessageModel<Answer>>(response);
                    var cancel = req!.Cancel ?? false;
                    if (cancel == false && agree!.Status == StatusCode.Success)
                    {
                        needNotify = true;
                        notify.Target = agree.Data;
                        notify.Type = NotifyType.AnswerAgree;
                        notify.Target_id = agree.Data!.Id;
                        var jwtHandler = new JwtSecurityTokenHandler();
                        var token = context.Request.Headers.Authorization.ToString();
                        if (token.IsNullOrEmpty() || !jwtHandler.CanReadToken(token))
                        {
                            var payload = MessageModel<string>.FailMsg("令牌不存在或者令牌错误", StatusCode.Redirect).ToJObject().ToString();

                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync(payload);
                            return;
                        }
                        var jwtToken = jwtHandler.ReadJwtToken(token);
                        notify.Opeate_id = int.Parse(jwtToken.Claims.SingleOrDefault(s => s.Type == "UserId")?.Value!);
                    }
                    break;
                default:
                    return;
            }

            if (needNotify)
            {
                _option.Helper!.Publish(JsonConvert.SerializeObject(notify), "NOTIFY");
            }
        }
    }
}

public  static class RabbitMqMiddlewareExtensions
{
    public static IApplicationBuilder UseRabbitMq(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RabbitMqMiddleware>();
    }

    public static IApplicationBuilder UseRabbitMq(this IApplicationBuilder builder, Action<RabbitMqOption> action)
    {
        var option = new RabbitMqOption();
        action(option);
        return builder.UseMiddleware<RabbitMqMiddleware>(Options.Create(option));
    }
}

public class RabbitMqOption
{
    public RabbitMqHelper? Helper { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RabbitMqAttribute : Attribute
{
    public string? ChannelName { get; set; }
    public NotifyType Type { get; set; }
}
