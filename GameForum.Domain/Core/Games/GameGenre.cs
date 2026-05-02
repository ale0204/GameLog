namespace GameForum.Domain.Core.Games;

public class GameGenre
{
    public Guid GameId { get; set; }
    public GameEntity Game { get; set; } = null!;

    public Guid GenreId { get; set; }
    public GenreEntity Genre { get; set; } = null!;
}
