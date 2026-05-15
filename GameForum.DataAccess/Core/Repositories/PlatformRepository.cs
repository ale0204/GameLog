using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Games;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class PlatformRepository : Repository<PlatformEntity>, IPlatformRepository
{
    public PlatformRepository(AppDbContext context) : base(context) { }

    public Task<PlatformEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
}
