using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Social;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class MessageRepository : Repository<MessageEntity>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context) { }

    public Task<List<MessageEntity>> GetConversationAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default) =>
        _set
            .Where(m => (m.SenderId == userA && m.ReceiverId == userB)
                     || (m.SenderId == userB && m.ReceiverId == userA))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
}
