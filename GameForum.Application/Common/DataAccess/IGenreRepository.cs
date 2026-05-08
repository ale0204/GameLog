using GameForum.Domain.Core.Games;

namespace GameForum.Application.Common.DataAccess;

public interface IGenreRepository : IRepository<GenreEntity>
{
    Task<GenreEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
