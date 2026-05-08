namespace GameForum.Application.Common.Models;

public class PostModel
{
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public double TrendingScore { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int VotesCount { get; set; }
}
