using GameForum.Domain.Common;
using GameForum.Domain.Core.Users;

namespace GameForum.Domain.Core.Badges;

public class UserBadgeEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid BadgeId { get; set; }
    public BadgeEntity Badge { get; set; } = null!;

    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
}
