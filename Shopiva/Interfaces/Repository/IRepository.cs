using System.Linq.Expressions;

namespace Shopiva.Interfaces.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

        Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default);

        Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default);

        Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken ct = default);


        Task<PaginatedResult<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false,
            CancellationToken ct = default);


        Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);


        IQueryable<TEntity> Query(bool noTracking = true);
    }
}
