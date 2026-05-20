using Shopiva.Contracts.Cart;

namespace Shopiva.Interfaces.Cart
{
    public interface ICartService
    {
        Task<Result<CartDto>> GetCartAsync(string userId, CancellationToken ct = default);
        Task<Result<CartDto>> AddItemAsync(string userId, AddToCartDto dto, CancellationToken ct = default);
        Task<Result<CartDto>> UpdateItemAsync(string userId, UpdateCartItemDto dto, CancellationToken ct = default);
        Task<Result<bool>> RemoveItemAsync(string userId, int cartItemId, CancellationToken ct = default);
        Task<Result> ClearCartAsync(string userId, CancellationToken ct = default);
    }
}
