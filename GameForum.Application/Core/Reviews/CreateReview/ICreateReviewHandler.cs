using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Reviews.CreateReview;

public interface ICreateReviewHandler
{
    Task<Response<ReviewModel>> HandleAsync(ReviewModel model, CancellationToken cancellationToken = default);
}
