using System.IdentityModel.Tokens.Jwt;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(DbHelper dbHelper, JwtHelper jwtHelper) : BaseController
{
    [HttpPost]
    public ActionResult<string> Login(Auth request)
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
    public ActionResult<string> Logout()
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var token = HttpContext.Request.Headers.Authorization;
        token = token.IsNullOrEmpty() ? "" : token.ToString().Replace("Bearer ", "");
        if (token.IsNullOrEmpty() || !jwtHandler.CanReadToken(token)) return Fail("token无效，请重新登录！");
        var jwtToken = jwtHandler.ReadJwtToken(token);
        var userId = jwtToken.Claims.SingleOrDefault(s => s.Type == "UserId")?.Value;
        jwtHelper.CreateToken(int.Parse(userId!));
        return Success("退出成功！");
    }
}
