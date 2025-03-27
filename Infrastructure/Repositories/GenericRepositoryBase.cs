using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class GenericRepositoryBase<TEntity> where TEntity : class
    {
        /// <summary>
        /// The memory cache
        /// </summary>
        protected readonly IMemoryCache _memoryCache;
        /// <summary>
        /// The context
        /// </summary>
        protected readonly ApplicationDbContext _context;

        // Toggle this to enable caching
        /// <summary>
        /// The include caching
        /// </summary>
        private readonly bool _includeCaching = true;

        /// <summary>
        /// The base cache key
        /// </summary>
        private static readonly string _baseCacheKey = typeof(GenericRepositoryBase<>).FullName ?? "GenericRepositoryBase";
        /// <summary>
        /// The cache keys
        /// </summary>
        protected static ConcurrentBag<string> _cacheKeys = new();

        /// <summary>
        /// The cache entry options
        /// </summary>
        private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.Normal
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepositoryBase{TEntity}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cache">The cache.</param>
        protected GenericRepositoryBase(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _memoryCache = cache;
        }

        /// <summary>
        /// Gets the or cache result asynchronous.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="executeQuery">The execute query.</param>
        /// <returns></returns>
        protected async Task<TReturn?> GetOrCacheResultAsync<TReturn>(
            IQueryable<TEntity> query,
            Func<Task<TReturn?>> executeQuery)
        {
            if (!_includeCaching)
                return await executeQuery();

            var cacheKey = GenerateCacheKey<TReturn>(query.ToQueryString());

            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetOptions(_cacheEntryOptions);
                return await executeQuery();
            });
        }

        /// <summary>
        /// Gets the or cache result.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="executeQuery">The execute query.</param>
        /// <returns></returns>
        protected TReturn? GetOrCacheResult<TReturn>(
            IQueryable<TEntity> query,
            Func<TReturn?> executeQuery)
        {
            if (!_includeCaching)
                return executeQuery();

            var cacheKey = GenerateCacheKey<TReturn>(query.ToQueryString());

            return _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.SetOptions(_cacheEntryOptions);
                return executeQuery();
            });
        }


        /// <summary>
        /// Generates the cache key.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="queryString">The query string.</param>
        /// <returns></returns>
        private string GenerateCacheKey<TReturn>(string queryString)
        {
            using var sha256 = SHA256.Create();

            var combined = $"{queryString}{typeof(TEntity).FullName}{typeof(TReturn).FullName}";
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            var cacheKey = $"{BitConverter.ToString(hash).Replace("-", "").ToLower()}-{typeof(TEntity).FullName}";

            if (_includeCaching && !_cacheKeys.Contains(cacheKey))
                _cacheKeys.Add(cacheKey);

            return cacheKey;
        }

        /// <summary>
        /// Builds the selector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected static Expression<Func<T, object>> BuildSelector<T>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.PropertyOrField(param, propertyName);
            var convert = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, param);
        }
    }
}
