using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Forum;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class VoteRepository : Repository<VoteEntity>, IVoteRepository
{
    public VoteRepository(AppDbContext context) : base(context) { }

    public Task<VoteEntity?> GetByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(v => v.UserId == userId && v.PostId == postId, cancellationToken);
}
