using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Forum;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class PostRepository : Repository<PostEntity>, IPostRepository
{
    public PostRepository(AppDbContext context) : base(context) { }

    public Task<List<PostEntity>> GetByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default) =>
        _set
            .Include(p => p.User)
            .Where(p => p.ThreadId == threadId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
}
