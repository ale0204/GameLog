using GameForum.Domain.Core.Games;

namespace GameForum.Application.Common.DataAccess;

public interface IGameRepository : IRepository<GameEntity>
{
    Task<GameEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<GameEntity?> GetByRawgIdAsync(int rawgId, CancellationToken cancellationToken = default);

    Task<List<GameEntity>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
}
