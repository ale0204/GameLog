using GameForum.Domain.Core.Users.Enums;

namespace GameForum.Application.Common.Models;

public class UserModel
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public bool IsProfilePublic { get; set; } = true;

    public Visibility DefaultGameVisibility { get; set; } = Visibility.Public;

    public Guid? CurrentlyPlayingGameId { get; set; }

    public Guid? FavoriteGameId { get; set; }

    public DateTime CreatedAt { get; set; }
}
