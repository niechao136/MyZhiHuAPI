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
    public async Task<PageModel<Answer>> List(AnswerPage request)
    {
        await using var conn = dbHelper.OpenConnection();
        var query = SqlHelper.AnswerList(request.Question_id, out var count);
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var answers = (await conn.QueryAsync<Answer>(query, new
        {
            limit = size,
            offset = (page - 1) * size
        })).ToList();
        var total = await conn.QueryFirstOrDefaultAsync<int>(count);
        return PageSuccess(answers, page, total, size);
    }

    [HttpPost]
    [RabbitMq(Type = NotifyType.Answer)]
    public async Task<MessageModel<Answer>> Add(AnswerCreate request)
    {
        await using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Answer>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var answer = await conn.QueryFirstOrDefaultAsync<Answer>(SqlHelper.AnswerInsert, new
        {
            questionId = request.Question_id,
            content = request.Content,
            ownerId
        });

        var question = await conn.QueryFirstOrDefaultAsync<Question>(SqlHelper.QuestionSet(false, "answers"), new
        {
            id = request.Question_id,
            target = answer!.Id
        });

        answer.Question = question;

        return Success("新增成功", answer);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public async Task<MessageModel<Answer>> Update(AnswerUpdate request)
    {
        await using var conn = dbHelper.OpenConnection();
        var answer = await conn.QueryFirstOrDefaultAsync<Answer>(SqlHelper.AnswerUpdate, new
        {
            content = request.Content,
            id = request.Id
        });
        return Success("修改成功", answer);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public async Task<MessageModel<string>> Delete(AnswerDelete request)
    {
        await using var conn = dbHelper.OpenConnection();
        await conn.ExecuteAsync(SqlHelper.AnswerDelete(), new { id = request.Id });
        await conn.ExecuteAsync(SqlHelper.CommitDelete("answer_id"), new { id = request.Id });
        return Success("删除成功", string.Empty);
    }

    [HttpPost]
    [RabbitMq(Type = NotifyType.AnswerAgree)]
    public async Task<MessageModel<Answer>> Agree(AnswerAgree request)
    {
        await using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Answer>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var question = await conn.QueryFirstOrDefaultAsync<Answer>(SqlHelper.AnswerSet(cancel, "agree"), new
        {
            id, ownerId
        });

        return Success((cancel ? "取消" : "") + "点赞成功", question);
    }
    
    [HttpPost]
    public async Task<MessageModel<Answer>> Remark(AnswerRemark request)
    {
        await using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Answer>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var answer = await conn.QueryFirstOrDefaultAsync<Answer>(SqlHelper.AnswerSet(cancel, "remark"), new
        {
            id, ownerId
        });

        var user = await conn.QueryFirstOrDefaultAsync<User>(SqlHelper.UserSet(cancel, "remarks"), new
        {
            id = ownerId,
            target = id
        });

        answer!.User = user;

        return Success((cancel ? "取消" : "") + "收藏成功", answer);
    }
}
