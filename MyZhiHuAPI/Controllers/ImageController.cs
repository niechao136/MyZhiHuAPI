using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Models;

namespace MyZhiHuAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ImageController(MinioHelper minioHelper) : BaseController
{
    [HttpPost]
    public async Task<MessageModel<string>> Upload(List<IFormFile> file)
    {
        var filePath = Path.GetTempFileName();
        await using var stream = System.IO.File.Create(filePath);
        await file[0].CopyToAsync(stream);
        stream.Position = 0;
        var md5 = new MD5CryptoServiceProvider();
        var fileMd5 = BitConverter.ToString(await md5.ComputeHashAsync(stream)).Replace("-", "").ToLower();
        var type = file[0].FileName.Split(".").Last();
        stream.Position = 0;
        var data = await minioHelper.PutAsync($"{fileMd5}.{type}", stream);
        return Success("成功", data);
    }
}
