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
        var query = SqlHelper.QuestionList(request.Owner_id, out var count);
        var questions = conn.Query<Question>(query, new
        {
            limit = size,
            offset = (page - 1) * size
        }).ToList();
        var total = conn.QueryFirstOrDefault<int>(count);
        return PageSuccess(questions, page, total, size);
    }

    [HttpPost]
    public MessageModel<Question> Add(QuestionCreate request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Question>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var question = conn.QueryFirstOrDefault<Question>(SqlHelper.QuestionInsert, new
        {
            title = request.Title,
            content = request.Content,
            ownerId
        });

        return Success("新增成功", question);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public MessageModel<Question> Update(QuestionUpdate request)
    {
        using var conn = dbHelper.OpenConnection();
        var question = conn.QueryFirstOrDefault<Question>(SqlHelper.QuestionUpdate, new
        {
            title = request.Title,
            content = request.Content,
            id = request.Id
        });
        return Success("修改成功", question);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public MessageModel<string> Delete(QuestionDelete request)
    {
        using var conn = dbHelper.OpenConnection();
        conn.Execute(SqlHelper.QuestionDelete, new { id = request.Id });
        return Success("删除成功", string.Empty);
    }

    [HttpPost]
    public MessageModel<Question> Watch(QuestionWatch request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Question>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var question = conn.QueryFirstOrDefault<Question>(SqlHelper.QuestionWatch(cancel, "watching"), new
        {
            id, ownerId
        });

        return Success((cancel ? "取消" : "") + "关注成功", question);
    }

}
