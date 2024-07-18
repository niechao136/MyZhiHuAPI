using Newtonsoft.Json.Linq;

namespace MyZhiHuAPI.Models;

/// <summary>
/// 通用返回信息类
/// </summary>
public class MessageModel<T>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Status { get; set; } = 200;

    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool IsSuccess { get; set; } = false;

    /// <summary>
    /// 返回信息
    /// </summary>
    public string Msg { get; set; } = "";

    /// <summary>
    /// 返回数据集合
    /// </summary>
    public T? Response { get; set; } = default(T);

    /// <summary>
    /// 返回消息字符串
    /// </summary>
    public override string ToString()
    {
        var code = IsSuccess ? "true" : "false";

        var data = new JObject
        {
            new JProperty("status", Status),
            new JProperty("success", code),
            new JProperty("msg", Msg)
        };

        if (Response != null) data.Add(new JProperty("data", Response));

        return data.ToString();
    }

    /// <summary>
    /// 返回消息
    /// </summary>
    /// <param name="success">失败/成功</param>
    /// <param name="msg">消息</param>
    /// <param name="response">数据</param>
    /// <param name="status">状态码</param>
    /// <returns></returns>
    public static MessageModel<T?> Message(bool success, string msg, T? response, int status = 200)
    {
        return new MessageModel<T?>
        {
            Msg = msg,
            Response = response,
            Status = status,
            IsSuccess = success
        };
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    /// <param name="msg">消息</param>
    /// <param name="response">数据</param>
    /// <returns></returns>
    public static string Success(string msg, T? response)
    {
        return Message(true, msg, response).ToString();
    }

    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="msg">消息</param>
    /// <param name="status">状态码</param>
    /// <returns></returns>
    public static string Fail(string msg, int status = 200)
    {
        return Message(false, msg, default, status).ToString();
    }
}
