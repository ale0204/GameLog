using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Library;

namespace GameForum.Application.Common.Mappings;

public static class UserGameModelMapping
{
    public static UserGameModel ToModel(this UserGameEntity entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        GameId = entity.GameId,
        Status = entity.Status,
        PersonalRating = entity.PersonalRating,
        HoursPlayed = entity.HoursPlayed,
        Notes = entity.Notes,
        StartedAt = entity.StartedAt,
        CompletedAt = entity.CompletedAt,
        Visibility = entity.Visibility,
        Game = entity.Game?.ToModel()
    };

    public static UserGameEntity ToEntity(this UserGameModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        UserId = model.UserId,
        GameId = model.GameId,
        Status = model.Status,
        PersonalRating = model.PersonalRating,
        HoursPlayed = model.HoursPlayed,
        Notes = model.Notes,
        StartedAt = model.StartedAt,
        CompletedAt = model.CompletedAt,
        Visibility = model.Visibility
    };

    public static void ApplyTo(this UserGameModel model, UserGameEntity entity)
    {
        entity.Status = model.Status;
        entity.PersonalRating = model.PersonalRating;
        entity.HoursPlayed = model.HoursPlayed;
        entity.Notes = model.Notes;
        entity.StartedAt = model.StartedAt;
        entity.CompletedAt = model.CompletedAt;
        entity.Visibility = model.Visibility;
    }

    public static List<UserGameModel> ToModelList(this IEnumerable<UserGameEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
