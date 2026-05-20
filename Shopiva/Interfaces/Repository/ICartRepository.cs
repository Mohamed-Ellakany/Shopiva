namespace Shopiva.Interfaces.Repository
{
    public interface ICartRepository : IRepository<Models.Cart>
    {
        Task<Models.Cart?> GetActiveCartWithItemsAsync(string userId, CancellationToken ct = default);
        Task<Models.Cart?> GetActiveCartWithItemsTrackedAsync(string userId, CancellationToken ct = default);
        Task<Models.Cart?> GetCartWithItemsAsync(int cartId, CancellationToken ct = default);
    }
}
