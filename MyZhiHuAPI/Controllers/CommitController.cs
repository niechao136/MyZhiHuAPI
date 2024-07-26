using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class CommitController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public PageModel<Commit> List(CommitPage request)
    {
        using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var answerId = request.Answer_id;
        var rootId = request.Root_id;
        var query =
            $"""
             SELECT commits.id, content, children, agree, answer_id, parent_id, owner_id,
                    root_id, commits.create_at, commits.update_at, users.nickname, parent.nickname AS parent
             FROM commits
                 LEFT JOIN users ON users.id = commits.owner_id
                 LEFT JOIN users AS parent ON users.id = commits.parent_id
             WHERE answer_id = @answerId AND root_id = @rootId
             ORDER BY update_at LIMIT {size} OFFSET {page * size - size}
             """;
        var commits = conn.Query<Commit>(query, new { answerId, rootId }).ToList();
        const string sql = "SELECT COUNT(DISTINCT id) FROM commits WHERE answer_id = @answer_id AND root_id = @rootId";
        var total = conn.QueryFirstOrDefault<int>(sql, new { answerId, rootId });
        return PageModel<Commit>.GetPage(true, page, total, size, commits);
    }

    [HttpPost]
    public MessageModel<Commit> Add(CommitCreate request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization);
        if (token == "token") return Fail<Commit>("token无效，请重新登录！");
        var ownerId = int.Parse(token);
        const string insert =
            """
            INSERT INTO commits (content, owner_id, answer_id, root_id, parent_id) 
            VALUES (@content, @ownerId, @answerId, @rootId, @parentId)
            RETURNING id, content, owner_id, answer_id, root_id, parent_id, update_at, create_at
            """;
        var answer = conn.QueryFirstOrDefault<Commit>(insert, new
        {
            parentId = request.Parent_id,
            rootId = request.Root_id,
            answerId = request.Answer_id,
            content = request.Content,
            ownerId
        });

        return Success("新增成功", answer);
    }
}
