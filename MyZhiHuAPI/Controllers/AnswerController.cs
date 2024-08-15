using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[MyAuthorize]
public class AnswerController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public PageModel<Answer> List(AnswerPage request)
    {
        using var conn = dbHelper.OpenConnection();
        var query = SqlHelper.AnswerList(request.Question_id, out var count);
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var answers = conn.Query<Answer>(query, new
        {
            limit = size,
            offset = (page - 1) * size
        }).ToList();
        var total = conn.QueryFirstOrDefault<int>(count);
        return PageSuccess(answers, page, total, size);
    }

    [HttpPost]
    [RabbitMq(Type = NotifyType.Answer)]
    public MessageModel<Answer> Add(AnswerCreate request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Answer>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var answer = conn.QueryFirstOrDefault<Answer>(SqlHelper.AnswerInsert, new
        {
            questionId = request.Question_id,
            content = request.Content,
            ownerId
        });

        return Success("新增成功", answer);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public MessageModel<Answer> Update(AnswerUpdate request)
    {
        using var conn = dbHelper.OpenConnection();
        var answer = conn.QueryFirstOrDefault<Answer>(SqlHelper.AnswerUpdate, new
        {
            content = request.Content,
            id = request.Id
        });
        return Success("修改成功", answer);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public MessageModel<string> Delete(AnswerDelete request)
    {
        using var conn = dbHelper.OpenConnection();
        conn.Execute(SqlHelper.AnswerDelete, new { id = request.Id });
        return Success("删除成功", string.Empty);
    }

    [HttpPost]
    [RabbitMq(Type = NotifyType.AnswerAgree)]
    public MessageModel<Answer> Agree(AnswerAgree request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Answer>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var question = conn.QueryFirstOrDefault<Answer>(SqlHelper.AnswerAgree(cancel, "agree"), new
        {
            id, ownerId
        });

        return Success((cancel ? "取消" : "") + "点赞成功", question);
    }
    
    [HttpPost]
    public MessageModel<Answer> Remark(AnswerRemark request)
    {
        using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Answer>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var question = conn.QueryFirstOrDefault<Answer>(SqlHelper.AnswerAgree(cancel, "remark"), new
        {
            id, ownerId
        });

        return Success((cancel ? "取消" : "") + "收藏成功", question);
    }
}
