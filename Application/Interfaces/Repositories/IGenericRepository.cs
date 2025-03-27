using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Queryable();

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        bool Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Detach(TEntity entity);
        void DetachRange(IEnumerable<TEntity> entities);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<int> TotalCountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<int> TotalCountAsync<TGroupBy>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TGroupBy>> groupBy, CancellationToken cancellationToken = default);
        int TotalCount(Expression<Func<TEntity, bool>>? predicate = null);

        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>>? predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, CancellationToken cancellationToken = default);

        Task<TResult?> GetAsync<TResult>(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            Expression<Func<TEntity, TResult>> selectExpression,
            CancellationToken cancellationToken = default);

        Task<TEntity?> GetAsync<TOrderBy>(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            Expression<Func<TEntity, TOrderBy>> orderBy,
            bool isDescending = false,
            CancellationToken cancellationToken = default);

        Task<TResult?> GetAsync<TOrderBy, TResult>(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            Expression<Func<TEntity, TResult>> selectExpression,
            Expression<Func<TEntity, TOrderBy>> orderBy,
            bool isDescending = false,
            CancellationToken cancellationToken = default);

        Task LoadReferenceAsync<TProperty>(
            TEntity entity,
            Expression<Func<TEntity, TProperty>> navigationProperty,
            CancellationToken cancellationToken = default)
            where TProperty : class;

        Task LoadCollectionAsync<TProperty>(
            TEntity entity,
            Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty,
            CancellationToken cancellationToken = default)
            where TProperty : class;

        void RemoveCacheKeys();
    }
}
