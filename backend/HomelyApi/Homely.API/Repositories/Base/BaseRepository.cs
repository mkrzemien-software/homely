using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;

namespace Homely.API.Repositories.Base;

public abstract class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> where TEntity : class
{
    protected readonly HomelyDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected BaseRepository(HomelyDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        await DbSet.AddRangeAsync(entityList, cancellationToken);
        return entityList;
    }

    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = DbSet.Update(entity);
        return Task.FromResult(entry.Entity);
    }

    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return false;

        return await DeleteAsync(entity, cancellationToken);
    }

    public virtual Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (HasSoftDelete(entity))
        {
            SetDeletedAt(entity, DateTimeOffset.UtcNow);
            DbSet.Update(entity);
        }
        else
        {
            DbSet.Remove(entity);
        }
        
        return Task.FromResult(true);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await ApplySoftDeleteFilter(DbSet).FirstOrDefaultAsync(GetIdPredicate(id), cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplySoftDeleteFilter(DbSet);
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.FirstOrDefaultAsync(GetIdPredicate(id));
    }

    public virtual async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await ApplySoftDeleteFilter(DbSet).FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplySoftDeleteFilter(DbSet);
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await ApplySoftDeleteFilter(DbSet).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplySoftDeleteFilter(DbSet);
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await ApplySoftDeleteFilter(DbSet).Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplySoftDeleteFilter(DbSet);
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.Where(predicate).ToListAsync();
    }

    public virtual IQueryable<TEntity> Query()
    {
        return ApplySoftDeleteFilter(DbSet);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await ApplySoftDeleteFilter(DbSet).AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = ApplySoftDeleteFilter(DbSet);
        return predicate == null
            ? await query.CountAsync(cancellationToken)
            : await query.CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySoftDeleteFilter(DbSet);

        if (predicate != null)
            query = query.Where(predicate);

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    protected abstract Expression<Func<TEntity, bool>> GetIdPredicate(TKey id);

    protected virtual bool HasSoftDelete(TEntity entity)
    {
        return entity.GetType().GetProperty("DeletedAt") != null;
    }
    
    protected virtual void SetDeletedAt(TEntity entity, DateTimeOffset deletedAt)
    {
        var property = entity.GetType().GetProperty("DeletedAt");
        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, deletedAt);
        }
    }

    protected virtual IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query)
    {
        var property = typeof(TEntity).GetProperty("DeletedAt");
        if (property != null)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var propertyAccess = Expression.Property(parameter, property);
            var nullConstant = Expression.Constant(null, property.PropertyType);
            var comparison = Expression.Equal(propertyAccess, nullConstant);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);

            query = query.Where(lambda);
        }

        return query;
    }
}
