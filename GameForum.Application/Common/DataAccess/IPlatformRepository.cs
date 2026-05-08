using GameForum.Domain.Core.Games;

namespace GameForum.Application.Common.DataAccess;

public interface IPlatformRepository : IRepository<PlatformEntity>
{
    Task<PlatformEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
