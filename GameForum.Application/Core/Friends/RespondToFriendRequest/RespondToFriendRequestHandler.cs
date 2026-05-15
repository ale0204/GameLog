using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Social.Enums;

namespace GameForum.Application.Core.Friends.RespondToFriendRequest;

public class RespondToFriendRequestHandler : IRespondToFriendRequestHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public RespondToFriendRequestHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<FriendshipModel>> HandleAsync(RespondToFriendRequestCommand command, CancellationToken cancellationToken = default)
    {
        var response = new Response<FriendshipModel>();
        try
        {
            // 1. Auth check (responder must be the authenticated user — placeholder)
            if (command.FriendshipId == Guid.Empty)
            {
                response.ErrorMessage = "FriendshipId is required";
                return response;
            }
            if (command.NewStatus == FriendshipStatus.Pending)
            {
                response.ErrorMessage = "NewStatus must be Accepted or Blocked";
                return response;
            }

            var entity = await _unitOfWork.Friendships.GetByIdAsync(command.FriendshipId, cancellationToken);
            if (entity is null)
            {
                response.ErrorMessage = "Friend request not found";
                return response;
            }
            if (entity.ReceiverId != command.ResponderId)
            {
                response.ErrorMessage = "Only the receiver can respond to this friend request";
                return response;
            }
            if (entity.Status != FriendshipStatus.Pending)
            {
                response.ErrorMessage = "Friend request is no longer pending";
                return response;
            }

            entity.Status = command.NewStatus;
            _unitOfWork.Friendships.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Data = entity.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
