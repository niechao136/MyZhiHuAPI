using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

public class BaseController : Controller
{

    [NonAction]
    protected static MessageModel<T> Success<T>(string message = "成功", T? response = default)
    {
        return MessageModel<T>.SuccessMsg(message, response!);
    }

    [NonAction]
    protected static MessageModel<T> Fail<T>(string message = "失败", int status = 500)
    {
        return MessageModel<T>.FailMsg(message, status);
    }

    [NonAction]
    protected static PageModel<T> PageSuccess<T>(List<T> data, int page, int total, int size, int status = 200)
    {
        return PageModel<T>.GetPage(true, page, total, size, data, status);
    }

    [NonAction]
    protected static string GetUserId(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        if (token.IsNullOrEmpty() || !jwtHandler.CanReadToken(token)) return "error";
        var jwtToken = jwtHandler.ReadJwtToken(token);
        return jwtToken.Claims.SingleOrDefault(s => s.Type == "UserId")?.Value!;
    }
}
