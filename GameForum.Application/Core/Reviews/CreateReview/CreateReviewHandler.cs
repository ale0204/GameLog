using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Reviews.CreateReview;

public class CreateReviewHandler : ICreateReviewHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public CreateReviewHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<ReviewModel>> HandleAsync(ReviewModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<ReviewModel>();
        try
        {
            // 1. Auth check
            if (model.UserId == Guid.Empty)
            {
                response.ErrorMessage = "UserId is required";
                return response;
            }
            if (model.GameId == Guid.Empty)
            {
                response.ErrorMessage = "GameId is required";
                return response;
            }
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                response.ErrorMessage = "Content is required";
                return response;
            }
            if (model.Rating < 1 || model.Rating > 10)
            {
                response.ErrorMessage = "Rating must be between 1 and 10";
                return response;
            }

            var userExists = await _unitOfWork.Users.ExistsAsync(model.UserId, cancellationToken);
            if (!userExists)
            {
                response.ErrorMessage = "User not found";
                return response;
            }
            var game = await _unitOfWork.Games.GetByIdAsync(model.GameId, cancellationToken);
            if (game is null)
            {
                response.ErrorMessage = "Game not found";
                return response;
            }

            var existing = await _unitOfWork.Reviews.GetByUserAndGameAsync(model.UserId, model.GameId, cancellationToken);
            if (existing is not null)
            {
                response.ErrorMessage = "User already reviewed this game";
                return response;
            }

            var entity = model.ToEntity();
            await _unitOfWork.Reviews.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
