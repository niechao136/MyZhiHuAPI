using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController(DbHelper dbHelper) : BaseController
{

    [HttpPost]
    public async Task<MessageModel<string>> Register(UserRegister request)
    {
        await using var conn = dbHelper.OpenConnection();
        var users = (await conn.QueryAsync<int>(SqlHelper.UserQuery, new { username = request.Username })).ToList();
        if (users.Count > 0) return Fail<string>("用户名已存在");
        await conn.ExecuteAsync(SqlHelper.UserInsert, new
        {
            username = request.Username,
            password = request.Password,
            role = request.Role,
            nickname = request.Nickname,
            email = request.Email,
            phone = request.Phone
        });
        return Success<string>("注册成功");
    }

    [HttpPost]
    [MyAuthorize]
    public async Task<MessageModel<User>> Info(UserInfo request)
    {
        var id = request.Id;
        if (request.Id == null)
        {
            var res = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
            if (res == "error") return Fail<User>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
            id = int.Parse(res);
        }

        await using var conn = dbHelper.OpenConnection();
        var users = (await conn.QueryAsync<User>(SqlHelper.UserInfo, new { id })).ToList();
        return users.Count == 0 ? Fail<User>("用户名不存在") : Success("获取成功", users[0]);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public async Task<MessageModel<User>> Add(UserRegister request)
    {
        await using var conn = dbHelper.OpenConnection();
        var users = (await conn.QueryAsync<int>(SqlHelper.UserQuery, new { username = request.Username })).ToList();
        if (users.Count > 0) return Fail<User>("用户名已存在");
        var user = await conn.QuerySingleOrDefaultAsync<User>(SqlHelper.UserAdd, new
        {
            username = request.Username,
            password = request.Password,
            role = request.Role,
            nickname = request.Nickname,
            email = request.Email,
            phone = request.Phone
        });
        
        return Success("新增成功", user);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public async Task<MessageModel<User>> Update(UserUpdate request)
    {
        await using var conn = dbHelper.OpenConnection();
        var user = await conn.QuerySingleOrDefaultAsync<User>(SqlHelper.UserUpdate, new
        {
            id = request.Id,
            nickname = request.Nickname,
            email = request.Email,
            phone = request.Phone
        });

        return Success("更新成功", user);
    }

    [HttpPost]
    [MyAuthorize(Roles = [UserRole.Admin])]
    public async Task<MessageModel<string>> Delete(UserDelete request)
    {
        await using var conn = dbHelper.OpenConnection();
        var user = await conn.QuerySingleOrDefaultAsync<User>(SqlHelper.UserDelete, new { id = request.Id });
        var watch = user!.Watching_people ?? [];
        var subscribers = user.Subscribers ?? [];
        if (watch.Length > 0)
        {
            await conn.ExecuteAsync(SqlHelper.UserSet(true, "subscribers", true), new
            {
                ids = watch,
                target = request.Id
            });
        }
        if (subscribers.Length > 0)
        {
            await conn.ExecuteAsync(SqlHelper.UserSet(true, "watching_people", true), new
            {
                ids = subscribers,
                target = request.Id
            });
        }
        return Success("删除成功", string.Empty);
    }

    [HttpPost]
    [MyAuthorize]
    public async Task<MessageModel<User>> Watch(UserWatch request)
    {
        await using var conn = dbHelper.OpenConnection();
        var token = GetUserId(HttpContext.Request.Headers.Authorization.ToString());
        if (token == "error") return Fail<User>("令牌不存在或者令牌错误", Models.StatusCode.Redirect);
        var id = int.Parse(token);
        var target = request.Id;
        var cancel = request.Cancel ?? false;
        var user = await conn.QueryFirstOrDefaultAsync<User>(SqlHelper.UserSet(cancel, "watching_people"), new
        {
            id,
            target
        });
        await conn.ExecuteAsync(SqlHelper.UserSet(cancel, "subscribers"), new
        {
            id = target,
            target = id
        });
        return Success((cancel ? "取消" : "") + "关注成功", user);
    }
}
