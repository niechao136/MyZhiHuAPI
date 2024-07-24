using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Models;
using Newtonsoft.Json.Linq;

namespace MyZhiHuAPI.Controllers;

public class BaseController : Controller
{

    [NonAction]
    public JObject Success<T>(T response, string message = "成功")
    {
        return MessageModel<T>.Success(message, response);
    }
    [NonAction]
    public JObject Success(string message = "成功")
    {
        return MessageModel<string>.Success(message, null);
    }

    [NonAction]
    public JObject Fail(string message = "失败", int status = 500)
    {
        return MessageModel<string>.Fail(message, status);
    }

    [NonAction]
    public JObject PageSuccess<T>(List<T> data, int page, int total, int size, int status = 200)
    {
        return PageModel<T>.GetPage(true, page, total, size, data, status).ToJObject();
    }

    [NonAction]
    public string GetUserId(StringValues token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var empty = token.IsNullOrEmpty() ? "" : token.ToString().Replace("Bearer ", "");
        if (empty.IsNullOrEmpty() || !jwtHandler.CanReadToken(empty)) return "token";
        var jwtToken = jwtHandler.ReadJwtToken(empty);
        return jwtToken.Claims.SingleOrDefault(s => s.Type == "UserId")?.Value!;
    }

}
