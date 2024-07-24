using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;
using Newtonsoft.Json.Linq;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(DbHelper dbHelper, JwtHelper jwtHelper) : BaseController
{
    [HttpPost]
    public ActionResult<JObject> Login(AuthLogin request)
    {
        using var conn = dbHelper.OpenConnection();
        const string query = "SELECT id FROM users WHERE username = @username AND password = @password";
        var users = conn.Query<int>(query, new { username = request.Username, password = request.Password }).ToList();
        if (users.Count > 0) return Success<string>(jwtHelper.CreateToken(users[0]));
        const string user = "SELECT id FROM users WHERE username = @username";
        users = conn.Query<int>(user, new { username = request.Username }).ToList();
        return Fail(users.Count > 0 ? "密码错误" : "用户不存在");
    }

    [Authorize]
    [HttpPost]
    public ActionResult<JObject> Logout()
    {
        var token = GetUserId(HttpContext.Request.Headers.Authorization);
        if (token == "token") return Fail("token无效，请重新登录！");
        jwtHelper.CreateToken(int.Parse(token));
        return Success("退出成功！");
    }
}
