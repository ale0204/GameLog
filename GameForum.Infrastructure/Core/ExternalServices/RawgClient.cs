using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GameForum.Application.Common.ExternalServices;
using GameForum.Application.Common.Logging;
using Microsoft.Extensions.Configuration;

namespace GameForum.Infrastructure.Core.ExternalServices;

public class RawgClient : IRawgClient
{
    private readonly HttpClient _httpClient;
    private readonly IAppLogger _logger;
    private readonly string _apiKey;

    public RawgClient(HttpClient httpClient, IConfiguration configuration, IAppLogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["RAWG:ApiKey"]
            ?? throw new InvalidOperationException("RAWG:ApiKey is not configured.");
    }

    public async Task<List<RawgGameDto>> SearchGamesAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var url = $"games?key={_apiKey}&search={Uri.EscapeDataString(query)}&page={page}&page_size={pageSize}";
        try
        {
            var response = await _httpClient.GetFromJsonAsync<RawgListResponse>(url, cancellationToken);
            if (response?.Results is null)
                return new List<RawgGameDto>();
            return response.Results.Select(MapListItem).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error($"RAWG search failed for '{query}': {ex.Message}");
            return new List<RawgGameDto>();
        }
    }

    public async Task<RawgGameDto?> GetGameDetailsAsync(int rawgId, CancellationToken cancellationToken = default)
    {
        var url = $"games/{rawgId}?key={_apiKey}";
        try
        {
            var detail = await _httpClient.GetFromJsonAsync<RawgGameDetail>(url, cancellationToken);
            return detail is null ? null : MapDetail(detail);
        }
        catch (Exception ex)
        {
            _logger.Error($"RAWG details failed for id {rawgId}: {ex.Message}");
            return null;
        }
    }

    private static RawgGameDto MapListItem(RawgGameSummary g) => new(
        Id: g.Id,
        Name: g.Name ?? string.Empty,
        BackgroundImage: g.BackgroundImage,
        Description: null,
        ReleaseYear: ParseYear(g.Released),
        Developer: null,
        Rating: g.Rating,
        PlayersCount: g.RatingsCount,
        Genres: g.Genres?.Select(x => x.Name ?? string.Empty).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new(),
        Platforms: g.Platforms?.Select(x => x.Platform?.Name ?? string.Empty).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new());

    private static RawgGameDto MapDetail(RawgGameDetail g) => new(
        Id: g.Id,
        Name: g.Name ?? string.Empty,
        BackgroundImage: g.BackgroundImage,
        Description: g.DescriptionRaw,
        ReleaseYear: ParseYear(g.Released),
        Developer: g.Developers?.FirstOrDefault()?.Name,
        Rating: g.Rating,
        PlayersCount: g.RatingsCount,
        Genres: g.Genres?.Select(x => x.Name ?? string.Empty).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new(),
        Platforms: g.Platforms?.Select(x => x.Platform?.Name ?? string.Empty).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new());

    private static int? ParseYear(string? released)
    {
        if (string.IsNullOrWhiteSpace(released)) return null;
        if (DateTime.TryParse(released, out var dt)) return dt.Year;
        if (released.Length >= 4 && int.TryParse(released[..4], out var y)) return y;
        return null;
    }

    // ---- internal DTOs matching RAWG JSON shape ----

    private sealed class RawgListResponse
    {
        [JsonPropertyName("results")]
        public List<RawgGameSummary>? Results { get; set; }
    }

    private sealed class RawgGameSummary
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("released")] public string? Released { get; set; }
        [JsonPropertyName("background_image")] public string? BackgroundImage { get; set; }
        [JsonPropertyName("rating")] public double Rating { get; set; }
        [JsonPropertyName("ratings_count")] public int RatingsCount { get; set; }
        [JsonPropertyName("genres")] public List<RawgNamed>? Genres { get; set; }
        [JsonPropertyName("platforms")] public List<RawgPlatformWrapper>? Platforms { get; set; }
    }

    private sealed class RawgGameDetail
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("released")] public string? Released { get; set; }
        [JsonPropertyName("background_image")] public string? BackgroundImage { get; set; }
        [JsonPropertyName("description_raw")] public string? DescriptionRaw { get; set; }
        [JsonPropertyName("rating")] public double Rating { get; set; }
        [JsonPropertyName("ratings_count")] public int RatingsCount { get; set; }
        [JsonPropertyName("developers")] public List<RawgNamed>? Developers { get; set; }
        [JsonPropertyName("genres")] public List<RawgNamed>? Genres { get; set; }
        [JsonPropertyName("platforms")] public List<RawgPlatformWrapper>? Platforms { get; set; }
    }

    private sealed class RawgNamed
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
    }

    private sealed class RawgPlatformWrapper
    {
        [JsonPropertyName("platform")] public RawgNamed? Platform { get; set; }
    }
}
