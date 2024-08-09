namespace MyZhiHuAPI.Models;

public class Notify<T>
{
    public int Id { get; set; }
    public int Owner_id { get; set; }
    public NotifyType Type { get; set; }
    public int Opeate_id { get; set; }
    public int Target_id { get; set; }
    public T? Target { get; set; }
    public string? Nickname { get; set; }
    public string? Title { get; set; }
}

public class NotifyAnswer
{
    public int User_id { get; set; }
    public string Nickname { get; set; }
    public string Title { get; set; }
}

public class NotifyAgree
{
    public string Nickname { get; set; }
}

public class NotifyCommit
{
    public int User_id { get; set; }
    public string Nickname { get; set; }
    public string Parent { get; set; }
    public string Owner { get; set; }
}

public enum NotifyType
{
    Answer = 1,
    Commit = 2,
    AnswerAgree = 3
}
