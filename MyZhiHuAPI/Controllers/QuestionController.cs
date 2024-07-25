using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class QuestionController(DbHelper dbHelper) : BaseController
{

    [HttpPost]
    public PageModel<Question> List(PageRequest request)
    {
        using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var query =
            $"""
             SELECT id, title, content, answers, watching, owner_id, create_at, update_at
             FROM questions ORDER BY update_at LIMIT {size} OFFSET {page * size - size}
             """;
        var questions = conn.Query<Question>(query).ToList();
        var total = conn.QueryFirstOrDefault<int>("SELECT COUNT(DISTINCT id) FROM questions");
        return PageModel<Question>.GetPage(true, page, total, size, questions);
    }

    [HttpPost]
    public MessageModel<Question> Add(QuestionCreate request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization);
        if (token == "token") return Fail<Question>("token无效，请重新登录！");
        var ownerId = int.Parse(token);
        const string insert =
            """
            INSERT INTO questions (title, content, owner_id) 
            VALUES (@title, @content, @ownerId)
            RETURNING id, title, content, owner_id, answers, watching, update_at, create_at
            """;
        var question = conn.QueryFirstOrDefault<Question>(insert, new
        {
            title = request.Title,
            content = request.Content,
            ownerId
        });

        return Success("新增成功", question);
    }
}
