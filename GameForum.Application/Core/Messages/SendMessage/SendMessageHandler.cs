using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Social.Enums;

namespace GameForum.Application.Core.Messages.SendMessage;

public class SendMessageHandler : ISendMessageHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public SendMessageHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<MessageModel>> HandleAsync(MessageModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<MessageModel>();
        try
        {
            // 1. Auth check (sender must be the authenticated user — placeholder)
            if (model.SenderId == Guid.Empty)
            {
                response.ErrorMessage = "SenderId is required";
                return response;
            }
            if (model.ReceiverId == Guid.Empty)
            {
                response.ErrorMessage = "ReceiverId is required";
                return response;
            }
            if (model.SenderId == model.ReceiverId)
            {
                response.ErrorMessage = "Cannot send a message to yourself";
                return response;
            }
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                response.ErrorMessage = "Content is required";
                return response;
            }

            var relation = await _unitOfWork.Friendships.GetRelationAsync(model.SenderId, model.ReceiverId, cancellationToken);
            if (relation is null || relation.Status != FriendshipStatus.Accepted)
            {
                response.ErrorMessage = "You can only send messages to accepted friends";
                return response;
            }

            var entity = model.ToEntity();
            await _unitOfWork.Messages.AddAsync(entity, cancellationToken);
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
