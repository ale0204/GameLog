using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Library;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class UserGameRepository : Repository<UserGameEntity>, IUserGameRepository
{
    public UserGameRepository(AppDbContext context) : base(context) { }

    public Task<List<UserGameEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _set
            .Include(ug => ug.Game)
            .Where(ug => ug.UserId == userId)
            .OrderByDescending(ug => ug.UpdatedAt)
            .ToListAsync(cancellationToken);

    public Task<UserGameEntity?> GetByUserAndGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId, cancellationToken);
}
