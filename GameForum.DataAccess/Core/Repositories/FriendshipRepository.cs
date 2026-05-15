using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Social.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class FriendshipRepository : Repository<FriendshipEntity>, IFriendshipRepository
{
    public FriendshipRepository(AppDbContext context) : base(context) { }

    public Task<FriendshipEntity?> GetRelationAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(
            f => (f.SenderId == userA && f.ReceiverId == userB)
              || (f.SenderId == userB && f.ReceiverId == userA),
            cancellationToken);

    public Task<List<FriendshipEntity>> GetAcceptedFriendsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _set
            .Where(f => f.Status == FriendshipStatus.Accepted
                     && (f.SenderId == userId || f.ReceiverId == userId))
            .ToListAsync(cancellationToken);
}
