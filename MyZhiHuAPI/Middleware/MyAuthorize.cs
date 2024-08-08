using System.IdentityModel.Tokens.Jwt;
using CSRedis;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Middleware;

public class MyAuthorizeMiddleware(RequestDelegate next, IOptions<MyAuthorizeOption> option)
{
    private readonly MyAuthorizeOption _option = option.Value;

    public async Task Invoke(HttpContext context)
    {
        var myAuthorize = context.GetEndpoint()!.Metadata.GetMetadata<MyAuthorizeAttribute>();
        if (myAuthorize == null) await next(context);
        var msg = "";
        var jwtHandler = new JwtSecurityTokenHandler();
        var token = context.Request.Headers.Authorization.ToString();
        var roles = myAuthorize?.Roles?.Split(',');
        if (token.IsNullOrEmpty() || !jwtHandler.CanReadToken(token)) msg = "令牌不存在或者令牌错误";
        else if (!await _option.CsRedisClient!.ExistsAsync(token)) msg = "令牌已过期";
        else if (roles != null && roles.Length != 0)
        {

        }

        if (msg != "")
        {
            var payload = MessageModel<string>.FailMsg(msg).ToJObject().ToString();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(payload);
            return;
        }

        var jwtToken = jwtHandler.ReadJwtToken(token);
        var id = int.Parse(jwtToken.Claims.SingleOrDefault(s => s.Type == "UserId")?.Value!);
        await _option.CsRedisClient!.ExpireAsync($"user_id:{id}", TimeSpan.FromMinutes(30));
        await _option.CsRedisClient!.ExpireAsync(token, TimeSpan.FromMinutes(30));

        await next(context);
    }
}

public static class MyAuthorizeMiddlewareExtensions
{
    public static IApplicationBuilder UseMyAuthorize(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyAuthorizeMiddleware>();
    }

    public static IApplicationBuilder UseMyAuthorize(this IApplicationBuilder builder, Action<MyAuthorizeOption> action)
    {
        var option = new MyAuthorizeOption();
        action(option);
        return builder.UseMiddleware<MyAuthorizeMiddleware>(Options.Create(option));
    }
}

public class MyAuthorizeOption
{
    public CSRedisClient? CsRedisClient { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class MyAuthorizeAttribute : Attribute
{
    public string? Roles { get; set; }
}
