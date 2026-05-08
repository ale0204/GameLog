using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Messages.GetConversation;

public record GetConversationQuery(Guid UserA, Guid UserB);

public interface IGetConversationHandler
{
    Task<Response<List<MessageModel>>> HandleAsync(GetConversationQuery query, CancellationToken cancellationToken = default);
}
