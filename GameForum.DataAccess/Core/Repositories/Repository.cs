using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _set.ToListAsync(cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity) => _set.Update(entity);

    public virtual void Delete(T entity) => _set.Remove(entity);
}
