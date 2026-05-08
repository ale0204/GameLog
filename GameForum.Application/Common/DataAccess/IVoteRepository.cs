using GameForum.Domain.Core.Forum;

namespace GameForum.Application.Common.DataAccess;

public interface IVoteRepository : IRepository<VoteEntity>
{
    Task<VoteEntity?> GetByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
}
