using Microsoft.AspNetCore.Mvc;
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
        var data = await minioHelper.PutAsync(file[0].FileName, stream);
        return Success("成功", data);
    }
}
