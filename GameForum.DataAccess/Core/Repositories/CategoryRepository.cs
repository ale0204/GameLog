using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Forum;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class CategoryRepository : Repository<CategoryEntity>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public override Task<List<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _set.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(cancellationToken);
}
