using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Games;

namespace GameForum.Application.Common.Mappings;

public static class GameModelMapping
{
    public static GameModel ToModel(this GameEntity entity) => new()
    {
        Id = entity.Id,
        RawgId = entity.RawgId,
        Title = entity.Title,
        CoverUrl = entity.CoverUrl,
        Description = entity.Description,
        ReleaseYear = entity.ReleaseYear,
        Developer = entity.Developer,
        AverageRating = entity.AverageRating,
        TotalPlayers = entity.TotalPlayers,
        Genres = entity.GameGenres
            .Where(gg => gg.Genre != null)
            .Select(gg => gg.Genre.Name)
            .ToList(),
        Platforms = entity.GamePlatforms
            .Where(gp => gp.Platform != null)
            .Select(gp => gp.Platform.Name)
            .ToList()
    };

    public static GameEntity ToEntity(this GameModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        RawgId = model.RawgId,
        Title = model.Title,
        CoverUrl = model.CoverUrl,
        Description = model.Description,
        ReleaseYear = model.ReleaseYear,
        Developer = model.Developer,
        AverageRating = model.AverageRating,
        TotalPlayers = model.TotalPlayers
    };

    public static List<GameModel> ToModelList(this IEnumerable<GameEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
