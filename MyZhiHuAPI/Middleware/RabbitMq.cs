using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using MyZhiHuAPI.Helpers;

namespace MyZhiHuAPI.Middleware;

public class RabbitMqMiddleware(RequestDelegate next, IOptions<RabbitMqOption> option)
{

    private readonly RabbitMqOption _option = option.Value;

    public async Task Invoke(HttpContext context)
    {
        var rabbitMq = context.GetEndpoint()!.Metadata.GetMetadata<RabbitMqAttribute>();
        if (rabbitMq == null) await next(context);
        var jwtHandler = new JwtSecurityTokenHandler();
        var token = context.Request.Headers.Authorization.ToString();
        var steam = new StreamReader(context.Request.Body);
        var body = await steam.ReadToEndAsync();
        _option.Helper!.Publish(body, rabbitMq?.ChannelName ?? "");
        await next(context);
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
}
