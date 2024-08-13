using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController(DbHelper dbHelper) : BaseController
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
            INSERT INTO users (username, password, role, nickname, email, phone) 
            VALUES (@username, @password, @role, @nickname, @email, @phone)
            """;
        conn.Execute(insert, new
        {
            username = request.Username,
            password = request.Password,
            role = request.Role,
            nickname = request.Nickname,
            email = request.Email,
            phone = request.Phone
        });
        return Success<string>("注册成功");
    }

    [HttpPost]
    [MyAuthorize]
    public MessageModel<User> Info(UserInfo request)
    {
        var id = request.Id;
        if (request.Id == null)
        {
            var res = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
            if (res == "error") return Fail<User>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
            id = int.Parse(res);
        }
        using var conn = dbHelper.OpenConnection();
        const string query =
            """
            SELECT id, username, role, nickname, questions, answers, commits, remarks,
            watching_people, watching_question, update_at FROM users WHERE id = @id
            """;
        var users = conn.Query<User>(query, new { id }).ToList();
        return users.Count == 0 ? Fail<User>("用户名不存在") : Success("获取成功", users[0]);
    }
}
