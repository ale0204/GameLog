namespace GameForum.Application.Common.Models;

public class ReviewModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int Rating { get; set; }

    public int LikesCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
