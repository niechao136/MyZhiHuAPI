using Dapper;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.Common;
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
    public async Task<PageModel<Question>> List(QuestionPage request)
    {
        await using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var query = SqlHelper.QuestionList(request.Owner_id, request.Keyword, out var count);
        var questions = (await conn.QueryAsync<Question>(query, new
        {
            limit = size,
            offset = (page - 1) * size,
        })).ToList();
        var total = await conn.QuerySingleOrDefaultAsync<int>(count);
        return PageSuccess(questions, page, total, size);
    }

    [HttpPost]
    public async Task<MessageModel<Question>> Add(QuestionCreate request)
    {
        await using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Question>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var ownerId = int.Parse(token);
        var segmenter = new JiebaSegmenter();
        var title = segmenter.CutForSearch(request.Title);
        var content = segmenter.CutForSearch(request.Content);
        var vector = title.Union(content).Join(" ");
        var question = await conn.QuerySingleOrDefaultAsync<Question>(SqlHelper.QuestionInsert, new
        {
            title = request.Title,
            content = request.Html,
            ownerId,
            search = vector
        });
        var user = await conn.QuerySingleOrDefaultAsync<User>(SqlHelper.UserSet(false, "questions"), new
        {
            id = ownerId,
            target = question!.Id
        });

        question.User = user;

        return Success("新增成功", question);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public async Task<MessageModel<Question>> Update(QuestionUpdate request)
    {
        await using var conn = dbHelper.OpenConnection();
        var question = await conn.QuerySingleOrDefaultAsync<Question>(SqlHelper.QuestionUpdate, new
        {
            title = request.Title,
            content = request.Content,
            id = request.Id
        });
        return Success("修改成功", question);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public async Task<MessageModel<string>> Delete(QuestionDelete request)
    {
        await using var conn = dbHelper.OpenConnection();
        var question = await conn.QuerySingleOrDefaultAsync<Question>(SqlHelper.QuestionDelete, new { id = request.Id });
        await conn.ExecuteAsync(SqlHelper.UserSet(true, "questions"), new
        {
            id = question!.Owner_id,
            target = request.Id
        });
        var watch = question.Watching ?? [];
        if (watch.Length > 0)
        {
            await conn.ExecuteAsync(SqlHelper.UserSet(true, "watching_question", true), new
            {
                ids = watch,
                target = question.Id
            });
        }
        await conn.ExecuteAsync(SqlHelper.AnswerDelete("question_id"), new { id = question.Id  });
        var ids = question.Answers ?? [];
        if (ids.Length > 0)
        {
            await conn.ExecuteAsync(SqlHelper.CommitDelete("answer_id", true), new { ids });
        }
        return Success("删除成功", string.Empty);
    }

    [HttpPost]
    public async Task<MessageModel<Question>> Watch(QuestionWatch request)
    {
        await using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<Question>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var target = int.Parse(token);
        var id = request.Id;
        var cancel = request.Cancel ?? false;
        var question = await conn.QuerySingleOrDefaultAsync<Question>(SqlHelper.QuestionSet(cancel, "watching"), new
        {
            id, target
        });
        var user = await conn.QuerySingleOrDefaultAsync<User>(SqlHelper.UserSet(cancel, "watching_question"), new
        {
            id = target,
            target = (int)question!.Id!
        });

        question.User = user;

        return Success((cancel ? "取消" : "") + "关注成功", question);
    }

}
