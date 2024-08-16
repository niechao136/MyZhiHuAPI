using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[MyAuthorize]
public class CommitController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public async Task<PageModel<Commit>> List(CommitPage request)
    {
        await using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var answerId = request.Answer_id;
        var rootId = request.Root_id;
        var commits = (await conn.QueryAsync<Commit>(SqlHelper.CommitList, new
        {
            answerId, rootId, limit = size, offset = size * (page - 1)
        })).ToList();
        var total = await conn.QueryFirstOrDefaultAsync<int>(SqlHelper.CommitCount, new { answerId, rootId });
        return PageSuccess(commits, page, total, size);
    }

    [HttpPost]
    [RabbitMq(Type = NotifyType.Commit)]
    public async Task<MessageModel<Commit>> Add(CommitCreate request)
    {
        await using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Commit>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var answer = await conn.QueryFirstOrDefaultAsync<Commit>(SqlHelper.CommitInsert, new
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
