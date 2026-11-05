using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Manbaa.Application.Abstractions;

using Manbaa.Domain.Common;

namespace Manbaa.Infrastructure.Persistence.Repositories;

public class GenericRepository<TEntity>(ManbaaDbContext context) : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> dbSet = context.Set<TEntity>();

    private IQueryable<TEntity> Entities => dbSet;

    public IQueryable<TEntity> Query(bool asNoTracking = true, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = asNoTracking ? Entities.AsNoTracking() : Entities;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }

    public async ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        var entity = await dbSet.FindAsync(new object[] { id }, cancellationToken);

        return entity is ISoftDeletable { IsDeleted: true } ? null : entity;
    }

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => Entities.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = Entities.AsNoTracking();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => Entities.AnyAsync(predicate, cancellationToken);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default) => predicate is null ? Entities.CountAsync(cancellationToken) : Entities.CountAsync(predicate, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbSet.AddAsync(entity, cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => dbSet.AddRangeAsync(entities, cancellationToken);

    public void Update(TEntity entity) => dbSet.Update(entity);

    public void Remove(TEntity entity) => dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => dbSet.RemoveRange(entities);
}
