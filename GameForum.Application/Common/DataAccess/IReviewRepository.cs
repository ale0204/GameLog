using GameForum.Domain.Core.Reviews;

namespace GameForum.Application.Common.DataAccess;

public interface IReviewRepository : IRepository<ReviewEntity>
{
    Task<List<ReviewEntity>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<ReviewEntity?> GetByUserAndGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
}
