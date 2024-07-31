using CSRedis;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController(CSRedisClient csRedisClient, DbHelper dbHelper) : BaseController(csRedisClient)
{

    [HttpPost]
    public MessageModel<string> Register(UserRegister request)
    {
        using var conn = dbHelper.OpenConnection();
        const string query = "SELECT id FROM users WHERE username = @username";
        var users = conn.Query<int>(query, new { username = request.Username }).ToList();
        if (users.Count > 0) return Fail<string>("用户名已存在");
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
        return Success<string>("注册成功");
    }

    [HttpPost]
    [Authorize]
    public MessageModel<User> Info(UserInfo request)
    {
        var id = request.Id;
        if (id == null)
        {
            var token = GetUserId(HttpContext.Request.Headers.Authorization);
            if (token == "token") return Fail<User>("token无效，请重新登录！");
            id = int.Parse(token);
        }
        using var conn = dbHelper.OpenConnection();
        const string query =
            """
            SELECT id, username, nickname, questions, answers, commits, remarks,
            watching_people, watching_question, update_at FROM users WHERE id = @id
            """;
        var users = conn.Query<User>(query, new { id }).ToList();
        return users.Count == 0 ? Fail<User>("用户名不存在") : Success("获取成功", users[0]);
    }
}
