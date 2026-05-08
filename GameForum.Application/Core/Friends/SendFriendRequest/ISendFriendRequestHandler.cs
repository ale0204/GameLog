using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Friends.SendFriendRequest;

public record SendFriendRequestCommand(Guid SenderId, Guid ReceiverId);

public interface ISendFriendRequestHandler
{
    Task<Response<FriendshipModel>> HandleAsync(SendFriendRequestCommand command, CancellationToken cancellationToken = default);
}
