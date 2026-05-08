using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Games.SearchGames;

public record SearchGamesQuery(string Query, int Page = 1, int PageSize = 20);

public interface ISearchGamesHandler
{
    Task<Response<List<GameModel>>> HandleAsync(SearchGamesQuery query, CancellationToken cancellationToken = default);
}
