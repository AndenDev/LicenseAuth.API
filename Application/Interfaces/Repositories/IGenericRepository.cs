using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Queryables this instance.
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> Queryable();

        /// <summary>
        /// Adds the asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Adds the range asynchronous.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Update(TEntity entity);
        /// <summary>
        /// Updates the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        bool Delete(TEntity entity);
        /// <summary>
        /// Deletes the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void DeleteRange(IEnumerable<TEntity> entities);
        /// <summary>
        /// Deletes the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Detaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Detach(TEntity entity);
        /// <summary>
        /// Detaches the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void DetachRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Existses the asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Totals the count asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<int> TotalCountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Totals the count asynchronous.
        /// </summary>
        /// <typeparam name="TGroupBy">The type of the group by.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="groupBy">The group by.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<int> TotalCountAsync<TGroupBy>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TGroupBy>> groupBy, CancellationToken cancellationToken = default);
        /// <summary>
        /// Totals the count.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        int TotalCount(Expression<Func<TEntity, bool>>? predicate = null);

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="include">The include.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>>? predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="include">The include.</param>
        /// <param name="selectExpression">The select expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TResult?> GetAsync<TResult>(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            Expression<Func<TEntity, TResult>> selectExpression,
            CancellationToken cancellationToken = default);

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
        Task<TEntity?> GetAsync<TOrderBy>(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            Expression<Func<TEntity, TOrderBy>> orderBy,
            bool isDescending = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TOrderBy">The type of the order by.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="include">The include.</param>
        /// <param name="selectExpression">The select expression.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="isDescending">if set to <c>true</c> [is descending].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TResult?> GetAsync<TOrderBy, TResult>(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            Expression<Func<TEntity, TResult>> selectExpression,
            Expression<Func<TEntity, TOrderBy>> orderBy,
            bool isDescending = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads the reference asynchronous.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task LoadReferenceAsync<TProperty>(
            TEntity entity,
            Expression<Func<TEntity, TProperty>> navigationProperty,
            CancellationToken cancellationToken = default)
            where TProperty : class;

        /// <summary>
        /// Loads the collection asynchronous.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task LoadCollectionAsync<TProperty>(
            TEntity entity,
            Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty,
            CancellationToken cancellationToken = default)
            where TProperty : class;

        /// <summary>
        /// Removes the cache keys.
        /// </summary>
        void RemoveCacheKeys();
    }
}
