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
}

public class QuestionPage: PageRequest
{
    public int? Owner_id { get; set; }
}

public class QuestionCreate
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}

public class QuestionWatch
{
    public int Id { get; set; }
    public bool? Cancel { get; set; }
}
