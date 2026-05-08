using GameForum.Domain.Core.Forum;

namespace GameForum.Application.Common.DataAccess;

public interface IPostRepository : IRepository<PostEntity>
{
    Task<List<PostEntity>> GetByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default);
}
