using GameForum.Domain.Common;

namespace GameForum.Domain.Core.Games;

public class GenreEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
}
