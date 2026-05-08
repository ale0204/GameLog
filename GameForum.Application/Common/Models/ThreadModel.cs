namespace GameForum.Application.Common.Models;

public class ThreadModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public Guid CategoryId { get; set; }

    public Guid? GameId { get; set; }

    public bool IsPinned { get; set; }

    public bool IsLocked { get; set; }

    public double TrendingScore { get; set; }

    public DateTime LastActivityAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int PostsCount { get; set; }

    public int VotesCount { get; set; }
}
