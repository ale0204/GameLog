namespace GameForum.Application.Common.Models;

public class GameModel
{
    public Guid Id { get; set; }

    public int RawgId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? CoverUrl { get; set; }

    public string? Description { get; set; }

    public int? ReleaseYear { get; set; }

    public string? Developer { get; set; }

    public double AverageRating { get; set; }

    public int TotalPlayers { get; set; }

    public List<string> Genres { get; set; } = new();

    public List<string> Platforms { get; set; } = new();
}
