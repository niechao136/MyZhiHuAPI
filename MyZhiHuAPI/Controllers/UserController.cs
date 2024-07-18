using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public ActionResult<string> Register(UserAdd request)
    {
        using var conn = dbHelper.OpenConnection();
        const string query = "SELECT id FROM users WHERE username = @username";
        var users = conn.Query<int>(query, new { username = request.Username }).ToList();
        if (users.Count > 0) return Fail("用户名已存在");
        const string insert =
            """
            INSERT INTO users (username, password, nickname, email, phone) 
            VALUES (@username, @password, @nickname, @email, @phone)
            """;
        conn.Execute(insert, new
        {
            username = request.Username,
            password = request.Password,
            nickname = request.Nickname,
            email = request.Email,
            phone = request.Phone
        });
        return Success("注册成功");
    }
}
