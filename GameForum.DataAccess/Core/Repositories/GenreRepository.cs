using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Games;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class GenreRepository : Repository<GenreEntity>, IGenreRepository
{
    public GenreRepository(AppDbContext context) : base(context) { }

    public Task<GenreEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
}
