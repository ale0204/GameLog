using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Friends.GetFriends;

public interface IGetFriendsHandler
{
    Task<Response<List<FriendshipModel>>> HandleAsync(Guid userId, CancellationToken cancellationToken = default);
}
