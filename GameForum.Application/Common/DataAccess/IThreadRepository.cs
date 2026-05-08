using GameForum.Domain.Core.Forum;

namespace GameForum.Application.Common.DataAccess;

public interface IThreadRepository : IRepository<ThreadEntity>
{
    Task<List<ThreadEntity>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<ThreadEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ThreadEntity>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<List<ThreadEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
