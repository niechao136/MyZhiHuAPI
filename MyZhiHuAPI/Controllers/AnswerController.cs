using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AnswerController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public PageModel<Answer> List(AnswerPage request)
    {
        using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var questionId = request.Question_id;
        var query =
            $"""
             SELECT id, content, commits, remark, question_id, owner_id, create_at, update_at
             FROM answers WHERE question_id = @questionId ORDER BY update_at LIMIT {size} OFFSET {page * size - size}
             """;
        var answers = conn.Query<Answer>(query, new { questionId }).ToList();
        const string sql = "SELECT COUNT(DISTINCT id) FROM answers WHERE question_id = @questionId";
        var total = conn.QueryFirstOrDefault<int>(sql, new { questionId });
        return PageModel<Answer>.GetPage(true, page, total, size, answers);
    }
}
