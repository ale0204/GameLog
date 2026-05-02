using GameForum.Domain.Common;
using GameForum.Domain.Core.Games;
using GameForum.Domain.Core.Users;

namespace GameForum.Domain.Core.Reviews;

public class ReviewEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid GameId { get; set; }
    public GameEntity Game { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public int Rating { get; set; }

    public int LikesCount { get; set; }
}
