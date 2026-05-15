using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Users;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class UserRepository : Repository<UserEntity>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set.AnyAsync(u => u.Id == id, cancellationToken);
}
