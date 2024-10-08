using Newtonsoft.Json;
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
    public StatusCode Status { get; set; } = StatusCode.Success;

    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// 返回信息
    /// </summary>
    public string Msg { get; set; } = "";

    /// <summary>
    /// 返回数据集合
    /// </summary>
    public T? Data { get; set; } = default(T);

    /// <summary>
    /// 返回消息字符串
    /// </summary>
    public JObject ToJObject()
    {
        var data = new JObject
        {
            new JProperty("status", Status),
            new JProperty("success", Success),
            new JProperty("msg", Msg)
        };

        if (Data != null) data.Add(new JProperty("data", Data is string ? Data : JsonConvert.DeserializeObject(Data.ToString()!)));

        return data;
    }

    /// <summary>
    /// 返回消息
    /// </summary>
    /// <param name="success">失败/成功</param>
    /// <param name="msg">消息</param>
    /// <param name="response">数据</param>
    /// <param name="status">状态码</param>
    /// <returns></returns>
    private static MessageModel<T> Message(bool success, string msg, StatusCode status = StatusCode.Success, T? response = default)
    {
        return new MessageModel<T>
        {
            Msg = msg,
            Data = response,
            Status = status,
            Success = success
        };
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    /// <param name="msg">消息</param>
    /// <param name="response">数据</param>
    /// <returns></returns>
    public static MessageModel<T> SuccessMsg(string msg, T response)
    {
        return Message(true, msg, StatusCode.Success, response);
    }

    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="msg">消息</param>
    /// <param name="status">状态码</param>
    /// <returns></returns>
    public static MessageModel<T> FailMsg(string msg, StatusCode status = StatusCode.Fail)
    {
        return Message(false, msg, status);
    }
}

public enum StatusCode
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 200,

    /// <summary>
    /// 直接返回登录页
    /// </summary>
    Redirect = 400,

    /// <summary>
    /// 跳出已登出提示
    /// </summary>
    Logout = 401,

    /// <summary>
    /// 失败
    /// </summary>
    Fail = 500
}
