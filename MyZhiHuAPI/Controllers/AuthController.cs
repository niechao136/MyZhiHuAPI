using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyZhiHuAPI.Helpers;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : Controller
{
    private readonly JwtHelper _jwtHelper;

    public AuthController(JwtHelper jwtHelper)
    {
        _jwtHelper = jwtHelper;
    }

    [HttpPost]
    public ActionResult<string> Login()
    {
        return _jwtHelper.CreateToken("1");
    }

    [Authorize]
    [HttpPost]
    public ActionResult<string> Test()
    {
        return "test";
    }
}
