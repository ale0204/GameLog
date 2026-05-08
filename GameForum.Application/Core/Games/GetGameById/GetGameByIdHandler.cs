using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Games.GetGameById;

public class GetGameByIdHandler : IGetGameByIdHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetGameByIdHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<GameModel>> HandleAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var response = new Response<GameModel>();
        try
        {
            // 1. Auth check (public)
            if (gameId == Guid.Empty)
            {
                response.ErrorMessage = "GameId is required";
                return response;
            }

            var entity = await _unitOfWork.Games.GetByIdWithDetailsAsync(gameId, cancellationToken);
            if (entity is null)
            {
                response.ErrorMessage = "Game not found";
                return response;
            }

            response.Data = entity.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
