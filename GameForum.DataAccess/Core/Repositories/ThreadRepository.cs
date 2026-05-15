using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Forum;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class ThreadRepository : Repository<ThreadEntity>, IThreadRepository
{
    public ThreadRepository(AppDbContext context) : base(context) { }

    public Task<List<ThreadEntity>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        _set
            .Include(t => t.User)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.IsPinned)
            // TODO: switch to TrendingScore once worker is wired up (Stage 5+)
            .ThenByDescending(t => t.LastActivityAt)
            .ToListAsync(cancellationToken);

    public Task<ThreadEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set
            .Include(t => t.User)
            .Include(t => t.Category)
            .Include(t => t.Game)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<List<ThreadEntity>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default) =>
        _set
            .Include(t => t.User)
            .Where(t => t.GameId == gameId)
            .OrderByDescending(t => t.LastActivityAt)
            .ToListAsync(cancellationToken);

    public Task<List<ThreadEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _set
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
}
