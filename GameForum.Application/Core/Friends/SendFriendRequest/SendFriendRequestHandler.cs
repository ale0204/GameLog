using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Social.Enums;

namespace GameForum.Application.Core.Friends.SendFriendRequest;

public class SendFriendRequestHandler : ISendFriendRequestHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public SendFriendRequestHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<FriendshipModel>> HandleAsync(SendFriendRequestCommand command, CancellationToken cancellationToken = default)
    {
        var response = new Response<FriendshipModel>();
        try
        {
            // 1. Auth check (sender must be the authenticated user — placeholder)
            if (command.SenderId == Guid.Empty)
            {
                response.ErrorMessage = "SenderId is required";
                return response;
            }
            if (command.ReceiverId == Guid.Empty)
            {
                response.ErrorMessage = "ReceiverId is required";
                return response;
            }
            if (command.SenderId == command.ReceiverId)
            {
                response.ErrorMessage = "Cannot send a friend request to yourself";
                return response;
            }

            var senderExists = await _unitOfWork.Users.ExistsAsync(command.SenderId, cancellationToken);
            if (!senderExists)
            {
                response.ErrorMessage = "Sender not found";
                return response;
            }
            var receiverExists = await _unitOfWork.Users.ExistsAsync(command.ReceiverId, cancellationToken);
            if (!receiverExists)
            {
                response.ErrorMessage = "Receiver not found";
                return response;
            }

            var existing = await _unitOfWork.Friendships.GetRelationAsync(command.SenderId, command.ReceiverId, cancellationToken);
            if (existing is not null)
            {
                response.ErrorMessage = "A friendship relation already exists between these users";
                return response;
            }

            var entity = new FriendshipEntity
            {
                SenderId = command.SenderId,
                ReceiverId = command.ReceiverId,
                Status = FriendshipStatus.Pending
            };
            await _unitOfWork.Friendships.AddAsync(entity, cancellationToken);
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
