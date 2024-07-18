using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

public class BaseController : Controller
{

    [NonAction]
    public string Success<T>(T response, string message = "成功")
    {
        return MessageModel<T>.Success(message, response);
    }
    [NonAction]
    public string Success(string message = "成功")
    {
        return MessageModel<string>.Success(message, null);
    }

    [NonAction]
    public string Fail(string message = "失败", int status = 500)
    {
        return MessageModel<string>.Fail(message, status);
    }

    [NonAction]
    public string PageSuccess<T>(List<T> data, int page, int total, int size, int status = 200)
    {
        return PageModel<T>.GetPage(true, page, total, size, data, status).ToString();
    }

}
