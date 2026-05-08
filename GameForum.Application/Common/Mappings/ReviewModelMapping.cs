using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Reviews;

namespace GameForum.Application.Common.Mappings;

public static class ReviewModelMapping
{
    public static ReviewModel ToModel(this ReviewEntity entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        GameId = entity.GameId,
        Content = entity.Content,
        Rating = entity.Rating,
        LikesCount = entity.LikesCount,
        CreatedAt = entity.CreatedAt
    };

    public static ReviewEntity ToEntity(this ReviewModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        UserId = model.UserId,
        GameId = model.GameId,
        Content = model.Content,
        Rating = model.Rating,
        LikesCount = model.LikesCount
    };

    public static void ApplyTo(this ReviewModel model, ReviewEntity entity)
    {
        entity.Content = model.Content;
        entity.Rating = model.Rating;
    }

    public static List<ReviewModel> ToModelList(this IEnumerable<ReviewEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
