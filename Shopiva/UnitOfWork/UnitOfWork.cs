using Shopiva.Interfaces.Repository;
using Shopiva.Interfaces.UnitOfWork;
using Shopiva.Repository;

namespace Shopiva.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private ICartRepository? _cartRepository = null;


        public ICartRepository Carts
        {
            get
            {
                if (_cartRepository is null)
                    _cartRepository = new CartRepository(_context);

                return _cartRepository;
            }
        }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
