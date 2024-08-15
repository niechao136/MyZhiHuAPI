using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;
using MyZhiHuAPI.Service;
using Newtonsoft.Json;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[MyAuthorize]
public class NotifyController(DbHelper dbHelper, WebsocketHelp websocketHelp, NotifyService notifyService) : BaseController
{
    [HttpPost]
    public async Task<PageModel<Notify<object>>> List(NotifyPage request)
    {
        await using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var list = (await conn.QueryAsync<Notify<object>>(SqlHelper.NotifyList, new
        {
            id = request.Owner_id,
            limit = size,
            offset = (page - 1) * size
        })).ToList();
        list = list.Select(item => notifyService.BuildAsync(item).Result).ToList();
        var total = await conn.ExecuteScalarAsync<int>(SqlHelper.NotifyCount, new { id = request.Owner_id });
        return PageSuccess(list, page, total, size);
    }

    [HttpPost]
    public async Task Last(NotifyPage request)
    {
        await using var conn = dbHelper.OpenConnection();
        var page = request.Page ?? 1;
        var size = request.Size ?? 10;
        var list = (await conn.QueryAsync<Notify<object>>(SqlHelper.NotifyList, new
        {
            id = request.Owner_id,
            limit = size,
            offset = (page - 1) * size
        })).ToList();
        foreach (var item in list)
        {
            var notify = await notifyService.BuildAsync(item);
            await websocketHelp.SendAsync(notify.Owner_id, JsonConvert.SerializeObject(notify));
        }
    }
}
