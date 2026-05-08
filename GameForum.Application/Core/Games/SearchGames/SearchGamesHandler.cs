using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Games.SearchGames;

public class SearchGamesHandler : ISearchGamesHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public SearchGamesHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<GameModel>>> HandleAsync(SearchGamesQuery query, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<GameModel>>();
        try
        {
            // 1. Auth check (public)
            // 2. Input validation
            if (string.IsNullOrWhiteSpace(query.Query))
            {
                response.ErrorMessage = "Search query cannot be empty";
                return response;
            }
            if (query.Page < 1)
            {
                response.ErrorMessage = "Page must be >= 1";
                return response;
            }
            if (query.PageSize < 1 || query.PageSize > 100)
            {
                response.ErrorMessage = "PageSize must be between 1 and 100";
                return response;
            }

            // 3. Search local DB cache (RAWG sync happens via SyncGameFromRawg)
            var entities = await _unitOfWork.Games.SearchAsync(query.Query.Trim(), query.Page, query.PageSize, cancellationToken);
            response.Data = entities.ToModelList();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
