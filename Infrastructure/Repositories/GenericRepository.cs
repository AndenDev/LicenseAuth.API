using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public partial class GenericRepository<TEntity>(
        ApplicationDbContext context,
        IMemoryCache memoryCache)
        : GenericRepositoryBase<TEntity>(context, memoryCache), IGenericRepository<TEntity>
        where TEntity : class
    {
        private static readonly string _baseCacheKey = typeof(TEntity).FullName ?? typeof(TEntity).Name;

        public IQueryable<TEntity> Queryable() => _context.Set<TEntity>();

        #region Create
        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            RemoveCacheKeys();
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            RemoveCacheKeys();
            await _context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        }
        #endregion

        #region Update
        public void Update(TEntity entity)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().UpdateRange(entities);
        }
        #endregion

        #region Delete
        public bool Delete(TEntity entity)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().Remove(entity);
            return true;
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
            return entity is not null && Delete(entity);
        }
        #endregion

        #region Detach
        public void Detach(TEntity entity)
            => _context.Entry(entity).State = EntityState.Detached;

        public void DetachRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Detach(entity);
        }
        #endregion

        #region Load Navigation
        public async Task LoadReferenceAsync<TProperty>(
            TEntity entity,
            Expression<Func<TEntity, TProperty>> navigationProperty,
            CancellationToken cancellationToken = default) where TProperty : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(navigationProperty);

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
                throw new InvalidOperationException("Entity must be attached to the context.");

            await entry.Reference(navigationProperty).LoadAsync(cancellationToken);
        }

        public async Task LoadCollectionAsync<TProperty>(
            TEntity entity,
            Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty,
            CancellationToken cancellationToken = default) where TProperty : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(navigationProperty);

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
                throw new InvalidOperationException("Entity must be attached to the context.");

            await entry.Collection(navigationProperty).LoadAsync(cancellationToken);
        }
        #endregion

        #region Count / Exists
        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().Where(predicate);
            return await GetOrCacheResultAsync(query, () => query.AnyAsync(cancellationToken));
        }

        public async Task<int> TotalCountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            if (predicate is not null)
                query = query.Where(predicate);

            return await GetOrCacheResultAsync(query, () => query.CountAsync(cancellationToken));
        }

        public int TotalCount(Expression<Func<TEntity, bool>>? predicate = null)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            if (predicate is not null)
                query = query.Where(predicate);

            return GetOrCacheResult(query, () => query.Count());
        }

        public async Task<int> TotalCountAsync<TGroupBy>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TGroupBy>> groupBy,
            CancellationToken cancellationToken = default)
        {
            var grouped = _context.Set<TEntity>()
                .Where(predicate)
                .GroupBy(groupBy)
                .Select(g => g.FirstOrDefault());

            var result = await grouped.ToListAsync(cancellationToken);
            return result?.Count ?? 0;
        }
        #endregion

        #region Get

        public async Task<TResult?> GetAsync<TResult>(
    Expression<Func<TEntity, bool>>? predicate,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
    Expression<Func<TEntity, TResult>> selectExpression,
    CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (predicate is not null)
                query = query.Where(predicate);

            if (include is not null)
                query = include(query);

            return await GetOrCacheResultAsync(query, () => query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken));
        }
        public async Task<TEntity?> GetAsync<TOrderBy>(
    Expression<Func<TEntity, bool>>? predicate,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
    Expression<Func<TEntity, TOrderBy>> orderBy,
    bool isDescending = false,
    CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (predicate is not null)
                query = query.Where(predicate);

            if (include is not null)
                query = include(query);

            query = isDescending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);

            return await GetOrCacheResultAsync(query, () => query.FirstOrDefaultAsync(cancellationToken));
        }


        public async Task<TEntity?> GetAsync(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            if (predicate is not null)
                query = query.Where(predicate);
            if (include is not null)
                query = include(query);

            return await GetOrCacheResultAsync(query, () => query.FirstOrDefaultAsync(cancellationToken));
        }

        public async Task<TResult?> GetAsync<TOrderBy, TResult>(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, TOrderBy>> orderBy,
            bool isDescending = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (predicate is not null)
                query = query.Where(predicate);
            if (include is not null)
                query = include(query);

            query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

            return await GetOrCacheResultAsync(query, () => query.Select(selector).FirstOrDefaultAsync(cancellationToken));
        }
        #endregion

        #region Cache
        public void RemoveCacheKeys()
        {
            var toRemove = _cacheKeys.Where(k => k.Contains(_baseCacheKey)).ToList();

            foreach (var key in toRemove)
                _memoryCache.Remove(key);

            _cacheKeys = new ConcurrentBag<string>(_cacheKeys.Except(toRemove));
        }
        #endregion
    }
}
