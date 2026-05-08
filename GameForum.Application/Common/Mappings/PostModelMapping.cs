using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Forum;

namespace GameForum.Application.Common.Mappings;

public static class PostModelMapping
{
    public static PostModel ToModel(this PostEntity entity) => new()
    {
        Id = entity.Id,
        ThreadId = entity.ThreadId,
        UserId = entity.UserId,
        Content = entity.Content,
        TrendingScore = entity.TrendingScore,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        VotesCount = entity.Votes.Sum(v => (int)v.Value)
    };

    public static PostEntity ToEntity(this PostModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        ThreadId = model.ThreadId,
        UserId = model.UserId,
        Content = model.Content,
        TrendingScore = model.TrendingScore,
        IsDeleted = model.IsDeleted
    };

    public static List<PostModel> ToModelList(this IEnumerable<PostEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
