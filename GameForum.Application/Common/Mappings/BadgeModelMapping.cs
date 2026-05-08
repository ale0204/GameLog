using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Badges;

namespace GameForum.Application.Common.Mappings;

public static class BadgeModelMapping
{
    public static BadgeModel ToModel(this BadgeEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        IconUrl = entity.IconUrl,
        Condition = entity.Condition
    };

    public static BadgeEntity ToEntity(this BadgeModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        Name = model.Name,
        Description = model.Description,
        IconUrl = model.IconUrl,
        Condition = model.Condition
    };
}
