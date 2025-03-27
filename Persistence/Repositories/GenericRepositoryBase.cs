using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Persistence.Repositories
{
    public abstract class GenericRepositoryBase<TEntity> where TEntity : class
    {
        protected readonly IMemoryCache _memoryCache;
        protected readonly AppDbContext _context;

        // Toggle this to enable caching
        private readonly bool _includeCaching = false;

        private static readonly string _baseCacheKey = typeof(GenericRepositoryBase<>).FullName ?? "GenericRepositoryBase";
        protected static ConcurrentBag<string> _cacheKeys = new();

        private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.Normal
        };

        protected GenericRepositoryBase(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _memoryCache = cache;
        }

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

        protected static Expression<Func<T, object>> BuildSelector<T>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.PropertyOrField(param, propertyName);
            var convert = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, param);
        }
    }
}
