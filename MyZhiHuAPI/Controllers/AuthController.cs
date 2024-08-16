using CSRedis;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(CSRedisClient csRedisClient, DbHelper dbHelper, JwtHelper jwtHelper) : BaseController
{

    [HttpPost]
    public async Task<MessageModel<string>> Login(AuthLogin request)
    {
        await using var conn = dbHelper.OpenConnection();
        var login = await conn.QueryFirstOrDefaultAsync<User>(SqlHelper.UserLogin, new
        {
            username = request.Username,
            password = request.Password
        });
        var id = login?.Id ?? -1;
        if (id != -1)
        {
            var token = jwtHelper.CreateToken(id, login?.Role ?? UserRole.User);
            if (await csRedisClient.ExistsAsync($"user_id:{id}"))
            {
                var oldToken = csRedisClient.Get($"user_id:{id}");
                await csRedisClient.DelAsync([oldToken!, $"user_id:{id}"]);
            }
            await csRedisClient.SetAsync($"user_id:{id}", token, TimeSpan.FromMinutes(30));
            await csRedisClient.SetAsync(token, id, TimeSpan.FromMinutes(30));
            return Success("成功", token);
        }
        var users = (await conn.QueryAsync<int>(SqlHelper.UserQuery, new { username = request.Username })).ToList();
        return Fail<string>(users.Count > 0 ? "密码错误" : "用户不存在");
    }

    [HttpPost]
    [MyAuthorize]
    public async Task<MessageModel<string>> Logout()
    {
        var token = HttpContext.Request.Headers.Authorization.ToString();
        var res = GetUserId(token);
        if (res == "error") return Fail<string>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var id = int.Parse(res);
        await csRedisClient.DelAsync([token, $"user_id:{id}"]);
        return Success<string>("退出成功", "");
    }
}
