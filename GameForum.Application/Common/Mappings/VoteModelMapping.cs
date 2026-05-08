using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Forum;

namespace GameForum.Application.Common.Mappings;

public static class VoteModelMapping
{
    public static VoteModel ToModel(this VoteEntity entity) => new()
    {
        Id = entity.Id,
        PostId = entity.PostId,
        UserId = entity.UserId,
        Value = entity.Value
    };

    public static VoteEntity ToEntity(this VoteModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        PostId = model.PostId,
        UserId = model.UserId,
        Value = model.Value
    };
}
