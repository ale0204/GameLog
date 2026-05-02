using GameForum.Domain.Common;
using GameForum.Domain.Core.Forum;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Reviews;

namespace GameForum.Domain.Core.Games;

public class GameEntity : BaseEntity
{
    public int RawgId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? CoverUrl { get; set; }

    public string? Description { get; set; }

    public int? ReleaseYear { get; set; }

    public string? Developer { get; set; }

    public double AverageRating { get; set; }

    public int TotalPlayers { get; set; }

    public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
    public ICollection<UserGameEntity> LibraryEntries { get; set; } = new List<UserGameEntity>();
    public ICollection<ThreadEntity> Threads { get; set; } = new List<ThreadEntity>();
    public ICollection<ReviewEntity> Reviews { get; set; } = new List<ReviewEntity>();
}
