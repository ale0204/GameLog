using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Reviews.UpdateReview;

public interface IUpdateReviewHandler
{
    Task<Response<ReviewModel>> HandleAsync(ReviewModel model, CancellationToken cancellationToken = default);
}
