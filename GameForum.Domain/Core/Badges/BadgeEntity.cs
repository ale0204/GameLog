using GameForum.Domain.Common;

namespace GameForum.Domain.Core.Badges;

public class BadgeEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }

    public string Condition { get; set; } = string.Empty;

    public ICollection<UserBadgeEntity> UserBadges { get; set; } = new List<UserBadgeEntity>();
}
