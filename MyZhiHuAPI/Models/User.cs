namespace MyZhiHuAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int[] Questions { get; set; }
    public int[] Answers { get; set; }
    public int[] Commits { get; set; }
    public int[] Remarks { get; set; }
    public int[] Watching_People { get; set; }
    public int[] Watching_Question { get; set; }
}

public class UserAdd
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
