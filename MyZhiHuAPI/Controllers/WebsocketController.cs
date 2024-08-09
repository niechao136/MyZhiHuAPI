using System.IdentityModel.Tokens.Jwt;
using CSRedis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class WebsocketController(WebsocketHelp websocketHelp, CSRedisClient redisClient) : BaseController
{
    // GET
    [HttpGet("{id:int?}")]
    public async Task Notify(int id, string? token)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentType = "application/json";
            var payload = MessageModel<string>.FailMsg("请使用 WebSocket 连接").ToJObject().ToString();
            await HttpContext.Response.WriteAsync(payload);
        }
        else
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var msg = "";
            var code = Models.StatusCode.Success;
            var jwtHandler = new JwtSecurityTokenHandler();
            if (token.IsNullOrEmpty() || !jwtHandler.CanReadToken(token))
            {
                msg = "令牌不存在或者令牌错误";
                code = Models.StatusCode.Redirect;
            }
            else if (!await redisClient.ExistsAsync(token))
            {
                msg = "令牌已过期";
                code = Models.StatusCode.Logout;
            }

            if (msg == "")
            {
                await websocketHelp.ReceiveAsync(id, webSocket);
            }

            var payload = msg == ""
                ? MessageModel<string>.SuccessMsg("连接成功", "").ToJObject().ToString()
                : MessageModel<string>.FailMsg(msg, code).ToJObject().ToString();
            await websocketHelp.SendAsync(id, payload, webSocket);

            if (msg != "") await WebsocketHelp.CloseAsync(webSocket);
        }
    }
}
