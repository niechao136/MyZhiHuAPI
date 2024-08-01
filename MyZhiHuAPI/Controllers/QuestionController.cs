using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[MyAuthorize]
public class QuestionController(DbHelper dbHelper) : BaseController
{

    [HttpPost]
    public PageModel<Question> List(QuestionPage request)
    {
        using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        const string isDelete = "is_delete = FALSE";
        var filter = request.Owner_id == null ? "" : $" AND owner_id = {request.Owner_id}";
        var query =
            $"""
             SELECT id, title, content, answers, watching, owner_id, create_at, update_at
             FROM questions WHERE {isDelete + filter} ORDER BY update_at LIMIT {size} OFFSET {page * size - size}
             """;
        var questions = conn.Query<Question>(query).ToList();
        var total = conn.QueryFirstOrDefault<int>($"SELECT COUNT(DISTINCT id) FROM questions WHERE {isDelete + filter}");
        return PageSuccess(questions, page, total, size);
    }

    [HttpPost]
    public MessageModel<Question> Add(QuestionCreate request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Question>("令牌不存在或者令牌错误");
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

    [HttpPost]
    public MessageModel<Question> Watch(QuestionWatch request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Question>("令牌不存在或者令牌错误");
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var action = cancel ? "array_remove" : "array_append";
        var update =
            $"""
             UPDATE questions SET watching = {action}(watching, {ownerId}::INTEGER) WHERE id = @id
             RETURNING id, title, content, owner_id, answers, watching, update_at, create_at
             """;
        var question = conn.QueryFirstOrDefault<Question>(update, new { id });

        return Success((cancel ? "取消" : "") + "关注成功", question);
    }

}
