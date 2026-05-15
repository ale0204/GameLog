using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Reviews.GetReviewsByGame;

public class GetReviewsByGameHandler : IGetReviewsByGameHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetReviewsByGameHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<ReviewModel>>> HandleAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<ReviewModel>>();
        try
        {
            // 1. Auth check (public endpoint)
            if (gameId == Guid.Empty)
            {
                response.ErrorMessage = "GameId is required";
                return response;
            }

            var reviews = await _unitOfWork.Reviews.GetByGameIdAsync(gameId, cancellationToken);
            response.Data = reviews.ToModelList();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
