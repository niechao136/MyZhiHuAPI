using Dapper;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;
using Newtonsoft.Json;

namespace MyZhiHuAPI.Service;

public class NotifyService(DbHelper dbHelper, WebsocketHelp websocketHelp)
{
    public async Task HandleNotify(NotifyType type, Notify<object> notify)
    {
        await using var conn = dbHelper.OpenConnection();
        switch (type)
        {
            case NotifyType.Answer:
                var answer = JsonConvert.DeserializeObject<Answer>(JsonConvert.SerializeObject(notify.Target));
                const string sql1 =
                    """
                    SELECT owner_id AS user_id, users.nickname, title FROM questions LEFT JOIN users ON users.id = @user_id
                    WHERE questions.id = @id
                    """;
                var res1 = await conn.QueryFirstOrDefaultAsync<NotifyAnswer>(sql1, new { id = answer!.Question_id, user_id = notify.Opeate_id });
                await websocketHelp.SendAsync(res1!.User_id, $"{res1.Nickname} 回答了您的问题 {res1.Title}");
                break;
            case NotifyType.Commit:
                var commit = JsonConvert.DeserializeObject<Commit>(JsonConvert.SerializeObject(notify.Target));
                const string sql3 =
                    """
                    SELECT owner_id AS user_id, users.nickname AS owner FROM answers
                    LEFT JOIN users ON users.id = @owner_id
                    WHERE answers.id = @id
                    """;
                var res3 = await conn.QueryFirstOrDefaultAsync<NotifyCommit>(sql3, new
                {
                    id = commit!.Answer_id,
                    owner_id = commit.Owner_id
                });
                await websocketHelp.SendAsync(res3!.User_id, $"{res3.Owner} 评论了您的回答");
                if (commit.Parent_id != -1)
                {
                    await websocketHelp.SendAsync(commit.Parent_id, $"{res3.Owner} 回复了您的评论");
                }
                break;
            case NotifyType.AnswerAgree:
                var agree = JsonConvert.DeserializeObject<Answer>(JsonConvert.SerializeObject(notify.Target));
                const string sql2 = "SELECT nickname FROM users WHERE id = @id";
                var res2 = await conn.QueryFirstOrDefaultAsync<NotifyAgree>(sql2, new { id = notify.Opeate_id });
                await websocketHelp.SendAsync(agree!.Owner_id, $"{res2!.Nickname} 赞同了您的回答");
                break;
            default:
                return;
        }
    }
}
