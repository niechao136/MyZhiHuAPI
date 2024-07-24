using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace MyZhiHuAPI.Models;

public class User
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Nickname { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int[]? Questions { get; set; }
    public int[]? Answers { get; set; }
    public int[]? Commits { get; set; }
    public int[]? Remarks { get; set; }
    public int[]? Watching_People { get; set; }
    public int[]? Watching_Question { get; set; }

    public override string ToString()
    {
        var data = new JObject
        {
            new JProperty("id", Id),
            new JProperty("username", Username),
            new JProperty("nickname", Nickname),
            new JProperty("questions", Questions.IsNullOrEmpty() ? [] : Questions),
            new JProperty("answers", Answers.IsNullOrEmpty() ? [] : Answers),
            new JProperty("commits", Commits.IsNullOrEmpty() ? [] : Commits),
            new JProperty("remarks", Remarks.IsNullOrEmpty() ? [] : Remarks),
            new JProperty("watching_people", Watching_People.IsNullOrEmpty() ? [] : Watching_People),
            new JProperty("watching_question", Watching_Question.IsNullOrEmpty() ? [] : Watching_Question)
        };
        return data.ToString();
    }
}

public class UserRegister
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Nickname { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class UserInfo
{
    public int? Id { get; set; }
}
