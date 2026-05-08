using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Users;

namespace GameForum.Application.Common.Mappings;

public static class UserModelMapping
{
    public static UserModel ToModel(this UserEntity entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        DisplayName = entity.DisplayName,
        AvatarUrl = entity.AvatarUrl,
        Bio = entity.Bio,
        LastSeenAt = entity.LastSeenAt,
        IsProfilePublic = entity.IsProfilePublic,
        DefaultGameVisibility = entity.DefaultGameVisibility,
        CurrentlyPlayingGameId = entity.CurrentlyPlayingGameId,
        FavoriteGameId = entity.FavoriteGameId,
        CreatedAt = entity.CreatedAt
    };

    public static UserEntity ToEntity(this UserModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        Username = model.Username,
        DisplayName = model.DisplayName,
        AvatarUrl = model.AvatarUrl,
        Bio = model.Bio,
        LastSeenAt = model.LastSeenAt,
        IsProfilePublic = model.IsProfilePublic,
        DefaultGameVisibility = model.DefaultGameVisibility,
        CurrentlyPlayingGameId = model.CurrentlyPlayingGameId,
        FavoriteGameId = model.FavoriteGameId
    };

    public static void ApplySettingsTo(this UserModel model, UserEntity entity)
    {
        entity.DisplayName = model.DisplayName;
        entity.AvatarUrl = model.AvatarUrl;
        entity.Bio = model.Bio;
        entity.IsProfilePublic = model.IsProfilePublic;
        entity.DefaultGameVisibility = model.DefaultGameVisibility;
        entity.CurrentlyPlayingGameId = model.CurrentlyPlayingGameId;
        entity.FavoriteGameId = model.FavoriteGameId;
    }
}
