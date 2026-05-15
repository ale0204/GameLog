using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;

namespace GameForum.Application.Core.Reviews.DeleteReview;

public class DeleteReviewHandler : IDeleteReviewHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public DeleteReviewHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<bool>> HandleAsync(DeleteReviewCommand command, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            // 1. Auth check
            if (command.ReviewId == Guid.Empty)
            {
                response.ErrorMessage = "Review Id is required";
                return response;
            }

            var entity = await _unitOfWork.Reviews.GetByIdAsync(command.ReviewId, cancellationToken);
            if (entity is null)
            {
                response.ErrorMessage = "Review not found";
                return response;
            }
            if (entity.UserId != command.RequesterId)
            {
                response.ErrorMessage = "Only the author can delete this review";
                return response;
            }

            // Hard delete — review history is not preserved
            _unitOfWork.Reviews.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Data = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
