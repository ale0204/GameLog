using GameForum.Domain.Core.Social;

namespace GameForum.Application.Common.DataAccess;

public interface IFriendshipRepository : IRepository<FriendshipEntity>
{
    Task<FriendshipEntity?> GetRelationAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);

    Task<List<FriendshipEntity>> GetAcceptedFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
}
