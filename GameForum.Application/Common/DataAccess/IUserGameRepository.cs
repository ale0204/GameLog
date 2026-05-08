using GameForum.Domain.Core.Library;

namespace GameForum.Application.Common.DataAccess;

public interface IUserGameRepository : IRepository<UserGameEntity>
{
    Task<List<UserGameEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserGameEntity?> GetByUserAndGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
}
