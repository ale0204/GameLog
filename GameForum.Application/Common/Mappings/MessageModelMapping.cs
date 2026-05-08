using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Social;

namespace GameForum.Application.Common.Mappings;

public static class MessageModelMapping
{
    public static MessageModel ToModel(this MessageEntity entity) => new()
    {
        Id = entity.Id,
        SenderId = entity.SenderId,
        ReceiverId = entity.ReceiverId,
        Content = entity.Content,
        IsRead = entity.IsRead,
        CreatedAt = entity.CreatedAt
    };

    public static MessageEntity ToEntity(this MessageModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        SenderId = model.SenderId,
        ReceiverId = model.ReceiverId,
        Content = model.Content,
        IsRead = model.IsRead
    };

    public static List<MessageModel> ToModelList(this IEnumerable<MessageEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
