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
        if (myAuthorize != null)
        {
            var msg = "";
            var code = StatusCode.Success;
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = context.Request.Headers.Authorization.ToString();
            var roles = myAuthorize.Roles;
            if (token.IsNullOrEmpty() || !jwtHandler.CanReadToken(token))
            {
                msg = "令牌不存在或者令牌错误";
                code = StatusCode.Redirect;
            }
            else if (!await _option.CsRedisClient!.ExistsAsync(token))
            {
                msg = "令牌已过期";
                code = StatusCode.Logout;
            }
            else if (roles != null && roles.Length != 0)
            {
                var jwt = jwtHandler.ReadJwtToken(token);
                var role = (UserRole)int.Parse(jwt.Claims.SingleOrDefault(s => s.Type == "UserRole")?.Value!);
                if (!roles.Contains(role))
                {
                    msg = "权限不足";
                    code = StatusCode.Fail;
                }
            }

            if (msg != "")
            {
                var payload = MessageModel<string>.FailMsg(msg, code).ToJObject().ToString();

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(payload);
                return;
            }

            var jwtToken = jwtHandler.ReadJwtToken(token);
            var id = int.Parse(jwtToken.Claims.SingleOrDefault(s => s.Type == "UserId")?.Value!);
            await _option.CsRedisClient!.ExpireAsync($"user_id:{id}", TimeSpan.FromMinutes(30));
            await _option.CsRedisClient!.ExpireAsync(token, TimeSpan.FromMinutes(30));
        }

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
    public UserRole[]? Roles { get; set; }
}
