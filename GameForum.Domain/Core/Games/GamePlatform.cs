namespace GameForum.Domain.Core.Games;

public class GamePlatform
{
    public Guid GameId { get; set; }
    public GameEntity Game { get; set; } = null!;

    public Guid PlatformId { get; set; }
    public PlatformEntity Platform { get; set; } = null!;
}
