using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Social;

namespace GameForum.Application.Common.Mappings;

public static class FriendshipModelMapping
{
    public static FriendshipModel ToModel(this FriendshipEntity entity) => new()
    {
        Id = entity.Id,
        SenderId = entity.SenderId,
        ReceiverId = entity.ReceiverId,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt
    };

    public static FriendshipEntity ToEntity(this FriendshipModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        SenderId = model.SenderId,
        ReceiverId = model.ReceiverId,
        Status = model.Status
    };

    public static List<FriendshipModel> ToModelList(this IEnumerable<FriendshipEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
