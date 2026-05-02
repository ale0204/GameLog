using GameForum.Domain.Common;
using GameForum.Domain.Core.Games;
using GameForum.Domain.Core.Library.Enums;
using GameForum.Domain.Core.Users;
using GameForum.Domain.Core.Users.Enums;

namespace GameForum.Domain.Core.Library;

public class UserGameEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid GameId { get; set; }
    public GameEntity Game { get; set; } = null!;

    public UserGameStatus Status { get; set; } = UserGameStatus.Wishlist;

    public int? PersonalRating { get; set; }

    public decimal? HoursPlayed { get; set; }

    public string? Notes { get; set; }

    public DateOnly? StartedAt { get; set; }

    public DateOnly? CompletedAt { get; set; }

    public Visibility Visibility { get; set; } = Visibility.Public;
}
