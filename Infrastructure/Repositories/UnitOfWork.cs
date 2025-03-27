using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context;
using System.Collections.Concurrent;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public UnitOfWork(DbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                var repoInstance = new GenericRepository<TEntity>(
                    (ApplicationDbContext)_context, 
                    _memoryCache
                );

                _repositories[type] = repoInstance;
            }

            return (IGenericRepository<TEntity>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
