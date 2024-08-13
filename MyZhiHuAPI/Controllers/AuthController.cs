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
    public MessageModel<string> Login(AuthLogin request)
    {
        using var conn = dbHelper.OpenConnection();
        const string query = "SELECT id, role FROM users WHERE username = @username AND password = @password";
        var login = conn.QueryFirstOrDefault<User>(query, new { username = request.Username, password = request.Password });
        var id = login?.Id ?? -1;
        if (id != -1)
        {
            var token = jwtHelper.CreateToken(id, login?.Role ?? UserRole.User);
            if (csRedisClient.Exists($"user_id:{id}"))
            {
                var oldToken = csRedisClient.Get($"user_id:{id}");
                csRedisClient.Del([oldToken!, $"user_id:{id}"]);
            }
            csRedisClient.Set($"user_id:{id}", token, TimeSpan.FromMinutes(30));
            csRedisClient.Set(token, id, TimeSpan.FromMinutes(30));
            return Success("成功", token);
        }
        const string user = "SELECT id FROM users WHERE username = @username";
        var users = conn.Query<int>(user, new { username = request.Username }).ToList();
        return Fail<string>(users.Count > 0 ? "密码错误" : "用户不存在");
    }

    [HttpPost]
    [MyAuthorize]
    public MessageModel<string> Logout()
    {
        var token = HttpContext.Request.Headers.Authorization.ToString();
        var res = GetUserId(token);
        if (res == "error") return Fail<string>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var id = int.Parse(res);
        csRedisClient.Del([token, $"user_id:{id}"]);
        return Success<string>("退出成功", "");
    }
}
