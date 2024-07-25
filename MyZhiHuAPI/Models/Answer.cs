namespace MyZhiHuAPI.Models;

public class Answer
{
    public int Id { get; set; }
    public int Owner_id { get; set; }
    public int Question_id { get; set; }
    public string Content { get; set; }
    public int[]? Commits { get; set; }
    public int[]? Agree { get; set; }
    public int[]? Remark { get; set; }
    public DateTime Create_at { get; set; }
    public DateTime Update_at { get; set; }
}

public class AnswerPage : PageRequest
{
    public int Question_id { get; set; }
}
