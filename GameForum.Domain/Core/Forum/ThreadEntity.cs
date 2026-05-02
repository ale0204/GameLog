using GameForum.Domain.Common;
using GameForum.Domain.Core.Games;
using GameForum.Domain.Core.Users;

namespace GameForum.Domain.Core.Forum;

public class ThreadEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public CategoryEntity Category { get; set; } = null!;

    public Guid? GameId { get; set; }
    public GameEntity? Game { get; set; }

    public bool IsPinned { get; set; }

    public bool IsLocked { get; set; }

    public double TrendingScore { get; set; }

    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public ICollection<PostEntity> Posts { get; set; } = new List<PostEntity>();
}
