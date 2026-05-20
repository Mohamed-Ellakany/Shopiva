using Shopiva.Interfaces.Repository;

namespace Shopiva.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ICartRepository Carts { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
