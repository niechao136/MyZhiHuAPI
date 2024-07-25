using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class CommitController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public PageModel<Commit> List(CommitPage request)
    {
        using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var answerId = request.Answer_id;
        var parentId = request.Parent_id;
        var query =
            $"""
             SELECT id, content, children, agree, answer_id, parent_id, owner_id, create_at, update_at
             FROM commits WHERE answer_id = @answerId AND parent_id = @parentId
             ORDER BY update_at LIMIT {size} OFFSET {page * size - size}
             """;
        var commits = conn.Query<Commit>(query, new { answerId, parentId }).ToList();
        const string sql = "SELECT COUNT(DISTINCT id) FROM commits WHERE answer_id = @answer_id AND parent_id = @parent_id";
        var total = conn.QueryFirstOrDefault<int>(sql, new { answerId, parentId });
        return PageModel<Commit>.GetPage(true, page, total, size, commits);
    }
}
