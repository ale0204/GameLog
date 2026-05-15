using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Games;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class GameRepository : Repository<GameEntity>, IGameRepository
{
    public GameRepository(AppDbContext context) : base(context) { }

    public Task<GameEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set
            .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public Task<GameEntity?> GetByRawgIdAsync(int rawgId, CancellationToken cancellationToken = default) =>
        _set
            .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
            .FirstOrDefaultAsync(g => g.RawgId == rawgId, cancellationToken);

    public Task<List<GameEntity>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{query}%";
        return _set
            .Where(g => EF.Functions.ILike(g.Title, pattern))
            .OrderBy(g => g.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
