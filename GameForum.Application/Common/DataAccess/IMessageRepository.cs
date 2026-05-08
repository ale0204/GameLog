using GameForum.Domain.Core.Social;

namespace GameForum.Application.Common.DataAccess;

public interface IMessageRepository : IRepository<MessageEntity>
{
    Task<List<MessageEntity>> GetConversationAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
}
