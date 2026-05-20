using Shopiva.Interfaces.Repository;
using System.Linq.Expressions;

namespace Shopiva.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        /// <summary>Returns a queryable with soft-delete filter applied when applicable.</summary>
        private IQueryable<TEntity> ActiveSet(bool noTracking = true)
        {
            IQueryable<TEntity> query = noTracking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();

            // Automatic global soft-delete filtering via shadow/conventional property
            var prop = typeof(TEntity).GetProperty("IsDeleted");
            if (prop is not null && prop.PropertyType == typeof(bool))
            {
                var param = Expression.Parameter(typeof(TEntity), "e");
                var body = Expression.Equal(
                    Expression.Property(param, prop),
                    Expression.Constant(false));
                query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, param));
            }

            return query;
        }


        // Queries
        /*
         * When using an ORM like Entity Framework, passing a simple Func forces the
         * application to load all data into memory and filter it locally. Passing an Expression allows
         * Entity Framework to translate the lambda into a WHERE clause in SQL, filtering the data
         * directly on the database server
        */
        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _dbSet.FindAsync(id, ct);

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
            => await ActiveSet().ToListAsync(ct);

        public async Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default)
            => await ActiveSet().Where(predicate).ToListAsync(ct);

        public async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default)
            => await ActiveSet().FirstOrDefaultAsync(predicate, ct);

        public async Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default)
            => await ActiveSet().AnyAsync(predicate, ct);

        public async Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken ct = default)
            => predicate is null
                ? await ActiveSet().CountAsync(ct)
                : await ActiveSet().CountAsync(predicate, ct);

        // ── Pagination ─────────────────────────────────────────────────────────

        public async Task<PaginatedResult<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false,
            CancellationToken ct = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

            var query = ActiveSet();

            if (predicate is not null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync(ct);

            if (orderBy is not null)
                query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PaginatedResult<TEntity>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        // ── Commands ───────────────────────────────────────────────────────────

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
        {
            var entry = await _dbSet.AddAsync(entity, ct);
            return entry.Entity;
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
            => await _dbSet.AddRangeAsync(entities, ct);

        public void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            // Auto-stamp UpdatedAt if the property exists
            var updatedAt = typeof(TEntity).GetProperty("UpdatedAt");
            updatedAt?.SetValue(entity, DateTime.UtcNow);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Update(entity);
        }

        public void Remove(TEntity entity)
        {
            // Soft-delete if the entity supports it, otherwise hard-delete
            var isDeletedProp = typeof(TEntity).GetProperty("IsDeleted");
            if (isDeletedProp is not null)
            {
                isDeletedProp.SetValue(entity, true);
                var updatedAt = typeof(TEntity).GetProperty("UpdatedAt");
                updatedAt?.SetValue(entity, DateTime.UtcNow);
                Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Remove(entity);
        }

        // Custom Query

        public IQueryable<TEntity> Query(bool noTracking = true)
            => ActiveSet(noTracking);
    }
}
