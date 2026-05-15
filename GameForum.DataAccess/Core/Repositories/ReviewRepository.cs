using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Reviews;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class ReviewRepository : Repository<ReviewEntity>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public Task<List<ReviewEntity>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default) =>
        _set
            .Include(r => r.User)
            .Where(r => r.GameId == gameId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<ReviewEntity?> GetByUserAndGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == gameId, cancellationToken);
}
