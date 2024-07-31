using CSRedis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ServerController(CSRedisClient csRedisClient, DbHelper dbHelper) : BaseController(csRedisClient)
{
    [HttpPost]
    public MessageModel<string> Info()
    {
        var version = dbHelper.GetConfig("Version");
        return version.IsNullOrEmpty() ? Fail<string>("版本号未配置") : Success<string>(version!);
    }
}
