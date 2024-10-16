namespace MyZhiHuAPI.Models;

public class Question
{
    public int? Id { get; set; }
    public int? Owner_id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int[]? Answers { get; set; }
    public int[]? Watching { get; set; }
    public DateTime Create_at { get; set; }
    public DateTime Update_at { get; set; }
    public User? User { get; set; }
}

public class QuestionPage: PageRequest
{
    public int? Owner_id { get; set; }
}

public class QuestionCreate
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Html { get; set; }
}

public class QuestionUpdate
{
    public int? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}

public class QuestionDelete
{
    public int? Id { get; set; }
}

public class QuestionWatch
{
    public int Id { get; set; }
    public bool? Cancel { get; set; }
}
