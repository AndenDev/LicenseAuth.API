using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Infrastructure.Repositories.GenericRepositoryBase&lt;TEntity&gt;" />
    /// <seealso cref="Application.Interfaces.Repositories.IGenericRepository&lt;TEntity&gt;" />
    public partial class GenericRepository<TEntity>(
        ApplicationDbContext context,
        IMemoryCache memoryCache)
        : GenericRepositoryBase<TEntity>(context, memoryCache), IGenericRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// The base cache key
        /// </summary>
        private static readonly string _baseCacheKey = typeof(TEntity).FullName ?? typeof(TEntity).Name;

        /// <summary>
        /// Queryables this instance.
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> Queryable() => _context.Set<TEntity>();

        #region Create
        /// <summary>
        /// Adds the asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            RemoveCacheKeys();
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Adds the range asynchronous.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            RemoveCacheKeys();
            await _context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Update(TEntity entity)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().Update(entity);
        }

        /// <summary>
        /// Updates the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().UpdateRange(entities);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool Delete(TEntity entity)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().Remove(entity);
            return true;
        }

        /// <summary>
        /// Deletes the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            RemoveCacheKeys();
            _context.Set<TEntity>().RemoveRange(entities);
        }

        /// <summary>
        /// Deletes the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
            return entity is not null && Delete(entity);
        }
        #endregion

        #region Detach
        /// <summary>
        /// Detaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Detach(TEntity entity)
            => _context.Entry(entity).State = EntityState.Detached;

        /// <summary>
        /// Detaches the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void DetachRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Detach(entity);
        }
        #endregion

        #region Load Navigation
        /// <summary>
        /// Loads the reference asynchronous.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException">Entity must be attached to the context.</exception>
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

        /// <summary>
        /// Loads the collection asynchronous.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException">Entity must be attached to the context.</exception>
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
        /// <summary>
        /// Existses the asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().Where(predicate);
            return await GetOrCacheResultAsync(query, () => query.AnyAsync(cancellationToken));
        }

        /// <summary>
        /// Totals the count asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<int> TotalCountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            if (predicate is not null)
                query = query.Where(predicate);

            return await GetOrCacheResultAsync(query, () => query.CountAsync(cancellationToken));
        }

        /// <summary>
        /// Totals the count.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public int TotalCount(Expression<Func<TEntity, bool>>? predicate = null)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            if (predicate is not null)
                query = query.Where(predicate);

            return GetOrCacheResult(query, () => query.Count());
        }

        /// <summary>
        /// Totals the count asynchronous.
        /// </summary>
        /// <typeparam name="TGroupBy">The type of the group by.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="groupBy">The group by.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="include">The include.</param>
        /// <param name="selectExpression">The select expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TOrderBy">The type of the order by.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="include">The include.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="isDescending">if set to <c>true</c> [is descending].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
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


        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="include">The include.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TOrderBy">The type of the order by.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="include">The include.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="isDescending">if set to <c>true</c> [is descending].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Removes the cache keys.
        /// </summary>
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
