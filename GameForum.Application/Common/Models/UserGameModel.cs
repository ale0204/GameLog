using GameForum.Domain.Core.Library.Enums;
using GameForum.Domain.Core.Users.Enums;

namespace GameForum.Application.Common.Models;

public class UserGameModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public UserGameStatus Status { get; set; } = UserGameStatus.Wishlist;

    public int? PersonalRating { get; set; }

    public decimal? HoursPlayed { get; set; }

    public string? Notes { get; set; }

    public DateOnly? StartedAt { get; set; }

    public DateOnly? CompletedAt { get; set; }

    public Visibility Visibility { get; set; } = Visibility.Public;

    public GameModel? Game { get; set; }
}
