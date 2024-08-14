namespace MyZhiHuAPI.Models;

public class User
{
    public int? Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public UserRole? Role { get; set; }
    public string? Nickname { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int[]? Questions { get; set; }
    public int[]? Answers { get; set; }
    public int[]? Commits { get; set; }
    public int[]? Remarks { get; set; }
    public int[]? Watching_people { get; set; }
    public int[]? Watching_question { get; set; }
}

public class UserRegister
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public UserRole? Role { get; set; }
    public string? Nickname { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class UserUpdate
{
    public int Id { get; set; }
    public string? Nickname { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class UserInfo
{
    public int? Id { get; set; }
}


public class UserDelete
{
    public int? Id { get; set; }
}

public enum UserRole
{
    Admin = 1,
    User = 2,
}
