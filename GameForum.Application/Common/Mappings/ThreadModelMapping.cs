using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Forum;

namespace GameForum.Application.Common.Mappings;

public static class ThreadModelMapping
{
    public static ThreadModel ToModel(this ThreadEntity entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        UserId = entity.UserId,
        CategoryId = entity.CategoryId,
        GameId = entity.GameId,
        IsPinned = entity.IsPinned,
        IsLocked = entity.IsLocked,
        TrendingScore = entity.TrendingScore,
        LastActivityAt = entity.LastActivityAt,
        CreatedAt = entity.CreatedAt,
        PostsCount = entity.Posts.Count(p => !p.IsDeleted),
        VotesCount = entity.Posts.SelectMany(p => p.Votes).Sum(v => (int)v.Value)
    };

    public static ThreadEntity ToEntity(this ThreadModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        Title = model.Title,
        UserId = model.UserId,
        CategoryId = model.CategoryId,
        GameId = model.GameId,
        IsPinned = model.IsPinned,
        IsLocked = model.IsLocked,
        TrendingScore = model.TrendingScore,
        LastActivityAt = model.LastActivityAt == default ? DateTime.UtcNow : model.LastActivityAt
    };

    public static List<ThreadModel> ToModelList(this IEnumerable<ThreadEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
