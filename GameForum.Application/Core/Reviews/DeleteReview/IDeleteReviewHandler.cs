using GameForum.Application.Common.Errors;

namespace GameForum.Application.Core.Reviews.DeleteReview;

public record DeleteReviewCommand(Guid ReviewId, Guid RequesterId);

public interface IDeleteReviewHandler
{
    Task<Response<bool>> HandleAsync(DeleteReviewCommand command, CancellationToken cancellationToken = default);
}
