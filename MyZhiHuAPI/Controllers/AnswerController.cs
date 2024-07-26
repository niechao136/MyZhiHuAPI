using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class AnswerController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public PageModel<Answer> List(AnswerPage request)
    {
        using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var questionId = request.Question_id;
        const string isDelete = "answers.is_delete = FALSE";
        var filter = questionId == null ? "" : $" AND answers.question_id = {questionId}";
        var query =
            $"""
             SELECT answers.id, answers.content, commits, remark, question_id, answers.owner_id,
                    answers.create_at, answers.update_at, questions.title
             FROM answers LEFT JOIN questions ON questions.id = answers.question_id
             WHERE {isDelete + filter} ORDER BY update_at LIMIT {size} OFFSET {page * size - size}
             """;
        var answers = conn.Query<Answer>(query, new { questionId }).ToList();
        var total = conn.QueryFirstOrDefault<int>($"SELECT COUNT(DISTINCT id) FROM answers WHERE {isDelete + filter}");
        return PageModel<Answer>.GetPage(true, page, total, size, answers);
    }

    [HttpPost]
    public MessageModel<Answer> Add(AnswerCreate request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization);
        if (token == "token") return Fail<Answer>("token无效，请重新登录！");
        var ownerId = int.Parse(token);
        const string insert =
            """
            INSERT INTO answers (content, owner_id, question_id) 
            VALUES (@content, @ownerId, @questionId)
            RETURNING id, content, owner_id, question_id, commits, remark, update_at, create_at
            """;
        var answer = conn.QueryFirstOrDefault<Answer>(insert, new
        {
            questionId = request.Question_id,
            content = request.Content,
            ownerId
        });

        return Success("新增成功", answer);
    }

    [HttpPost]
    public MessageModel<Answer> Agree(AnswerAgree request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization);
        if (token == "token") return Fail<Answer>("token无效，请重新登录！");
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var action = cancel ? "array_remove" : "array_append";
        var update =
            $"""
             UPDATE answers SET agree = {action}(agree, {ownerId}::INTEGER) WHERE id = @id
             RETURNING id, content, owner_id, question_id, agree, remark, update_at, create_at
             """;
        var question = conn.QueryFirstOrDefault<Answer>(update, new { id });

        return Success((cancel ? "取消" : "") + "点赞成功", question);
    }
    
    [HttpPost]
    public MessageModel<Answer> Remark(AnswerAgree request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization);
        if (token == "token") return Fail<Answer>("token无效，请重新登录！");
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var action = cancel ? "array_remove" : "array_append";
        var update =
            $"""
             UPDATE answers SET remark = {action}(remark, {ownerId}::INTEGER) WHERE id = @id
             RETURNING id, content, owner_id, question_id, agree, remark, update_at, create_at
             """;
        var question = conn.QueryFirstOrDefault<Answer>(update, new { id });

        return Success((cancel ? "取消" : "") + "收藏成功", question);
    }
}
