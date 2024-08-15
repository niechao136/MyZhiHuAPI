using Dapper;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;
using Newtonsoft.Json;

namespace MyZhiHuAPI.Service;

public class NotifyService(DbHelper dbHelper, WebsocketHelp websocketHelp)
{
    public async Task HandleAsync(NotifyType type, Notify<object> notify)
    {
        await using var conn = dbHelper.OpenConnection();
        switch (type)
        {
            case NotifyType.Answer:
                var answer = JsonConvert.DeserializeObject<Answer>(JsonConvert.SerializeObject(notify.Target));
                await InsertAsync(async () =>
                {
                    var res = await conn.QueryFirstOrDefaultAsync<NotifyQuery>(SqlHelper.NotifyAnswer, new
                    {
                        id = answer!.Question_id,
                        owner_id = notify.Operate_id
                    });
                    res!.Operate_id = notify.Operate_id;
                    res.Type = notify.Type;
                    res.Target_id = notify.Target_id;
                    return res;
                }, async value =>
                {
                    value.Target = answer;
                    await websocketHelp.SendAsync(value.Owner_id, JsonConvert.SerializeObject(value));
                });
                break;
            case NotifyType.Commit:
                var commit = JsonConvert.DeserializeObject<Commit>(JsonConvert.SerializeObject(notify.Target));
                await InsertAsync(async () =>
                {
                    var res = await conn.QueryFirstOrDefaultAsync<NotifyQuery>(SqlHelper.NotifyCommit, new
                    {
                        id = commit!.Answer_id,
                        owner_id = commit.Owner_id
                    });
                    res!.Operate_id = notify.Operate_id;
                    res.Type = notify.Type;
                    res.Target_id = notify.Target_id;
                    return res;
                }, async value =>
                {
                    value.Target = commit;
                    await websocketHelp.SendAsync(value.Owner_id, JsonConvert.SerializeObject(value));
                    if (commit!.Parent_id != -1)
                    {
                        await websocketHelp.SendAsync(commit.Parent_id, JsonConvert.SerializeObject(value));
                    }
                });
                break;
            case NotifyType.AnswerAgree:
                var agree = JsonConvert.DeserializeObject<Answer>(JsonConvert.SerializeObject(notify.Target));
                await InsertAsync(async () =>
                {
                    var res = await conn.QueryFirstOrDefaultAsync<NotifyQuery>(SqlHelper.NotifyAgree, new
                    {
                        id = notify.Operate_id
                    });
                    res!.Owner_id = agree!.Owner_id;
                    res.Operate_id = notify.Operate_id;
                    res.Type = notify.Type;
                    res.Target_id = notify.Target_id;
                    return res;
                }, async value =>
                {
                    value.Target = agree;
                    await websocketHelp.SendAsync(value.Owner_id, JsonConvert.SerializeObject(value));
                });
                break;
            default:
                return;
        }
    }

    private async Task InsertAsync(Func<Task<NotifyQuery>> queryFunc, Func<Notify<object>, Task> sendFunc)
    {
        var query = await queryFunc();
        await using var conn = dbHelper.OpenConnection();
        var notify = await conn.QueryFirstOrDefaultAsync<Notify<object>>(SqlHelper.NotifyInsert, new
        {
            owner_id = query.Owner_id,
            operate_id = query.Operate_id,
            target_id = query.Target_id,
            type = (int)query.Type
        });
        notify!.Title = query.Title;
        notify.Nickname = query.Nickname;
        await sendFunc(notify);
    }

    public async Task<Notify<object>> BuildAsync(Notify<object> notify)
    {
        await using var conn = dbHelper.OpenConnection();
        switch (notify.Type)
        {
            case NotifyType.Answer:
                var answer = await conn.QueryFirstOrDefaultAsync<Answer>(SqlHelper.AnswerInfo, new
                {
                    id = notify.Target_id
                });
                var res1 = await conn.QueryFirstOrDefaultAsync<NotifyQuery>(SqlHelper.NotifyAnswer, new
                {
                    id = answer!.Question_id,
                    owner_id = notify.Operate_id
                });
                notify.Nickname = res1!.Nickname;
                notify.Target = answer;
                notify.Title = res1.Title;
                return notify;
            case NotifyType.Commit:
                var commit = await conn.QueryFirstOrDefaultAsync<Commit>(SqlHelper.CommitInfo, new
                {
                    id = notify.Target_id
                });
                var res2 = await conn.QueryFirstOrDefaultAsync<NotifyQuery>(SqlHelper.NotifyCommit, new
                {
                    id = commit!.Answer_id,
                    owner_id = commit.Owner_id
                });
                notify.Nickname = res2!.Nickname;
                notify.Target = commit;
                return notify;
            case NotifyType.AnswerAgree:
                var agree = await conn.QueryFirstOrDefaultAsync<Answer>(SqlHelper.AnswerInfo, new
                {
                    id = notify.Target_id
                });
                var res3 = await conn.QueryFirstOrDefaultAsync<NotifyQuery>(SqlHelper.NotifyAgree, new
                {
                    id = notify.Operate_id
                });
                notify.Nickname = res3!.Nickname;
                notify.Target = agree;
                return notify;
            default:
                return notify;
        }
    }
}
