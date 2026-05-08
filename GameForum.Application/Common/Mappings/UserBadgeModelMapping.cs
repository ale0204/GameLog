using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Badges;

namespace GameForum.Application.Common.Mappings;

public static class UserBadgeModelMapping
{
    public static UserBadgeModel ToModel(this UserBadgeEntity entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        BadgeId = entity.BadgeId,
        AwardedAt = entity.AwardedAt,
        Badge = entity.Badge?.ToModel()
    };
}
