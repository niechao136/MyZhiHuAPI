using System.Net.WebSockets;
using System.Text;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Helpers;

public class WebsocketHelp
{
    private readonly Dictionary<int, WebSocket> _sockets = [];

    public async Task ReceiveAsync(int id, WebSocket socket)
    {
        if (_sockets.TryGetValue(id, out var webSocket))
        {
            var payload = MessageModel<string>.FailMsg("用户已在其他设备登录", StatusCode.Logout).ToJObject().ToString();
            await SendAsync(id, payload, webSocket);
            await CloseAsync(webSocket);
            _sockets.Remove(id);
        }
        _sockets.Add(id, socket);

        var buffer = new byte[1024 * 10];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), default);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await CloseAsync(socket);
                break;
            }
            await socket.SendAsync(buffer[..result.Count], WebSocketMessageType.Text, true, default);
        }

        _sockets.Remove(id);
    }

    public async Task SendAsync(int id, string message, WebSocket? webSocket = null)
    {
        var socket = webSocket;
        if (socket != null || _sockets.TryGetValue(id, out socket))
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, default);
            }
        }
    }

    public static async Task CloseAsync(WebSocket webSocket)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default);
        }
    }
}
