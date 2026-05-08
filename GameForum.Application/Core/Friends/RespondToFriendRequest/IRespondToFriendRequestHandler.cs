using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Social.Enums;

namespace GameForum.Application.Core.Friends.RespondToFriendRequest;

public record RespondToFriendRequestCommand(Guid FriendshipId, FriendshipStatus NewStatus, Guid ResponderId);

public interface IRespondToFriendRequestHandler
{
    Task<Response<FriendshipModel>> HandleAsync(RespondToFriendRequestCommand command, CancellationToken cancellationToken = default);
}
