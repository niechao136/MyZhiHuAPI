using Minio;
using Minio.DataModel.Args;

namespace MyZhiHuAPI.Helpers;

public class MinioHelper(IConfiguration configuration)
{
    private readonly IMinioClient _minioClient = new MinioClient()
        .WithEndpoint(configuration["Minio:endpoint"])
        .WithCredentials(configuration["Minio:accessKey"], configuration["Minio:secretKey"])
        .Build();

    public async Task<string> PutAsync(string fileName, FileStream fileStream)
    {
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(configuration["Minio:bucketName"])
            .WithObjectSize(fileStream.Length)
            .WithObject(fileName)
            .WithStreamData(fileStream));
        return $"http://{configuration["Minio:endpoint"]}/{configuration["Minio:bucketName"]}/{fileName}";
    }

}
