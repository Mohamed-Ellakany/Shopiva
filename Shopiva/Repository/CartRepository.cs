using Shopiva.Abstractions.enums;
using Shopiva.Interfaces.Repository;

namespace Shopiva.Repository
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context)
        {}

        public async Task<Cart?> GetActiveCartWithItemsAsync(string userId, CancellationToken ct = default)
        {
            return await Query(noTracking: true).Include(c=>c.Items).FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active, ct);
        }

        public async Task<Cart?> GetActiveCartWithItemsTrackedAsync(string userId, CancellationToken ct = default)
        { 
            return await Query(noTracking: false)          // ← EF tracks everything
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active, ct);
        }

        public async Task<Cart?> GetCartWithItemsAsync(int cartId, CancellationToken ct = default)
        {
            return await Query(noTracking: true).Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == cartId, ct);
        }
    }
}
