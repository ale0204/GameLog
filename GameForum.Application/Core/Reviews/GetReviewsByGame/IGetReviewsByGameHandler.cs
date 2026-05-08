using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Reviews.GetReviewsByGame;

public interface IGetReviewsByGameHandler
{
    Task<Response<List<ReviewModel>>> HandleAsync(Guid gameId, CancellationToken cancellationToken = default);
}
