using Newtonsoft.Json.Linq;

namespace MyZhiHuAPI.Models;

/// <summary>
/// 通用返回列表类
/// </summary>
public class PageModel<T>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Status { get; set; } = 200;

    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// 当前页标
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 数据总数
    /// </summary>
    public int TotalCount { get; set; } = 0;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { set; get; } = 10;

    /// <summary>
    /// 返回数据
    /// </summary>
    public List<T> Data { get; set; } = [];

    /// <summary>
    /// 返回消息字符串
    /// </summary>
    public JObject ToJObject()
    {
        var data = new JObject
        {
            new JProperty("status", Status),
            new JProperty("success", Success),
            new JProperty("rows", Data),
            new JProperty("page", Page),
            new JProperty("total", TotalCount),
            new JProperty("size", PageSize)
        };

        return data;
    }

    /// <summary>
    /// 返回消息
    /// </summary>
    /// <param name="success">失败/成功</param>
    /// <param name="page">当前页标</param>
    /// <param name="total">数据总数</param>
    /// <param name="size">每页大小</param>
    /// <param name="data">数据</param>
    /// <param name="status">状态码</param>
    /// <returns></returns>
    public static PageModel<T> GetPage(bool success, int page, int total, int size, List<T> data, int status = 200)
    {
        return new PageModel<T>
        {
            Success = success,
            Status = status,
            Page = page,
            TotalCount = total,
            PageSize = size,
            Data = data
        };
    }
}

public class PageRequest
{
    public int? Page { get; set; } = 1;
    public int? Size { get; set; } = 10;
    public string? Sort { get; set; }
    public string? Order { get; set; }
}
