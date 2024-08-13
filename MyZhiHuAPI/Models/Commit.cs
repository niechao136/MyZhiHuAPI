namespace MyZhiHuAPI.Models;

public class Commit
{
    public int Id { get; set; }
    public int Owner_id { get; set; }
    public int Answer_id { get; set; }
    public int Parent_id { get; set; }
    public int Root_id { get; set; }
    public string? Content { get; set; }
    public string? Nickname { get; set; }
    public string? Parent { get; set; }
    public int[]? Children { get; set; }
    public int[]? Agree { get; set; }
    public DateTime Create_at { get; set; }
    public DateTime Update_at { get; set; }
}

public class CommitPage : PageRequest
{
    public int Answer_id { get; set; }
    public int Root_id { get; set; }
}

public class CommitCreate
{
    public int Answer_id { get; set; }
    public int Root_id { get; set; }
    public int Parent_id { get; set; }
    public string? Content { get; set; }
}
