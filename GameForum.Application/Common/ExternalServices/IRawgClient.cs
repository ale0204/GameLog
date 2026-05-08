namespace GameForum.Application.Common.ExternalServices;

public interface IRawgClient
{
    Task<List<RawgGameDto>> SearchGamesAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<RawgGameDto?> GetGameDetailsAsync(int rawgId, CancellationToken cancellationToken = default);
}

public record RawgGameDto(
    int Id,
    string Name,
    string? BackgroundImage,
    string? Description,
    int? ReleaseYear,
    string? Developer,
    double Rating,
    int PlayersCount,
    List<string> Genres,
    List<string> Platforms);
