using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;
using Newtonsoft.Json.Linq;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public ActionResult<JObject> Register(UserRegister request)
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

    [HttpPost]
    [Authorize]
    public ActionResult<JObject> Info(UserInfo request)
    {
        var id = request.Id;
        if (id == null)
        {
            var token = GetUserId(HttpContext.Request.Headers.Authorization);
            if (token == "token") return Fail("token无效，请重新登录！");
            id = int.Parse(token);
        }
        using var conn = dbHelper.OpenConnection();
        const string query =
            """
            SELECT id, username, nickname, questions, answers, commits, remarks,
            watching_people, watching_question, update_at FROM users WHERE id = @id
            """;
        var users = conn.Query<User>(query, new { id }).ToList();
        return users.Count == 0 ? Fail("用户名不存在") : Success(users[0], "获取成功");
    }
}
