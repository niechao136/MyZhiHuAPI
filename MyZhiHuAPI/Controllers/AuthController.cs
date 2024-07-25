using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(DbHelper dbHelper, JwtHelper jwtHelper) : BaseController
{
    [HttpPost]
    public MessageModel<string> Login(AuthLogin request)
    {
        using var conn = dbHelper.OpenConnection();
        const string query = "SELECT id FROM users WHERE username = @username AND password = @password";
        var users = conn.Query<int>(query, new { username = request.Username, password = request.Password }).ToList();
        if (users.Count > 0) return Success("成功", jwtHelper.CreateToken(users[0]));
        const string user = "SELECT id FROM users WHERE username = @username";
        users = conn.Query<int>(user, new { username = request.Username }).ToList();
        return Fail<string>(users.Count > 0 ? "密码错误" : "用户不存在");
    }

    [Authorize]
    [HttpPost]
    public MessageModel<string> Logout()
    {
        var token = GetUserId(HttpContext.Request.Headers.Authorization);
        if (token == "token") return Fail<string>("token无效，请重新登录！");
        jwtHelper.CreateToken(int.Parse(token));
        return Success<string>("退出成功！", "");
    }
}
