using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Messages.SendMessage;

public interface ISendMessageHandler
{
    Task<Response<MessageModel>> HandleAsync(MessageModel model, CancellationToken cancellationToken = default);
}
