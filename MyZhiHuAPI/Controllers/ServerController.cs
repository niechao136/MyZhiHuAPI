using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Helpers;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ServerController(DbHelper dbHelper) : BaseController
{
    [HttpPost]
    public ActionResult<string> Info()
    {
        var version = dbHelper.GetConfig("Version");
        return version.IsNullOrEmpty() ? Fail("版本号未配置") : Success(version!);
    }
}
