using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Reviews.UpdateReview;

public class UpdateReviewHandler : IUpdateReviewHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public UpdateReviewHandler(IUnitOfWork unitOfWork, IAppLogger logger)
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
            if (model.Id == Guid.Empty)
            {
                response.ErrorMessage = "Review Id is required";
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

            var existing = await _unitOfWork.Reviews.GetByIdAsync(model.Id, cancellationToken);
            if (existing is null)
            {
                response.ErrorMessage = "Review not found";
                return response;
            }
            if (existing.UserId != model.UserId)
            {
                response.ErrorMessage = "Only the author can update this review";
                return response;
            }

            model.ApplyTo(existing);
            _unitOfWork.Reviews.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Data = existing.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
