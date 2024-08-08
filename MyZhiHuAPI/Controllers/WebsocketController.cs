using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class WebsocketController : BaseController
{
    // GET
    public async Task<MessageModel<string>> Notify()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest) return Fail<string>("请使用Websocket请求");
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        return Success<string>("通知成功");
    }
}
