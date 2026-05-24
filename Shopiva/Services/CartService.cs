using Shopiva.Abstractions.enums;
using Shopiva.Contracts.Cart;
using Shopiva.Interfaces.Cart;
using Shopiva.Interfaces.Redis;
using Shopiva.Interfaces.UnitOfWork;

namespace Shopiva.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisService _redis;

        public CartService(IUnitOfWork unitOfWork, IRedisService redis)
        {
            _unitOfWork = unitOfWork;
            _redis = redis;
        }

        public async Task<Result<CartDto>> GetCartAsync(string userId, CancellationToken ct = default)
        {
            // 1. Try Redis first
            var cached = await _redis.GetAsync<CartDto>($"cart:{userId}");
            if (cached.IsSuccess && cached.Value is not null)
                return Result<CartDto>.Success(cached.Value);

            // 2. Fallback to SQL (read-only, noTracking is fine here)
            var cart = await _unitOfWork.Carts.GetActiveCartWithItemsAsync(userId, ct);

            // 3. No cart yet → create one
            if (cart is null)
            {
                cart = new Cart { UserId = userId, Status = CartStatus.Active };
                await _unitOfWork.Carts.AddAsync(cart, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            // 4. Cache and return
            var dto = MapToDto(cart);
            await _redis.SetAsync($"cart:{userId}", dto);
            return Result<CartDto>.Success(dto);
        }

        public async Task<Result<CartDto>> AddItemAsync(string userId, AddToCartDto dto, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetActiveCartWithItemsTrackedAsync(userId, ct);

            if (cart is null)
            {
                cart = new Cart { UserId = userId, Status = CartStatus.Active };
                await _unitOfWork.Carts.AddAsync(cart, ct);
                await _unitOfWork.SaveChangesAsync(ct); // get the cart Id
            }

            var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existing is not null)
            {
                existing.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                });
            }

            await _unitOfWork.SaveChangesAsync(ct);

            await _redis.DeleteAsync($"cart:{userId}"); // invalidate cache

            // Re-fetch with items to build fresh DTO
            var updated = await _unitOfWork.Carts.GetActiveCartWithItemsAsync(userId, ct);
            var result = MapToDto(updated!);
            await _redis.SetAsync($"cart:{userId}", result);

            return Result<CartDto>.Success(result);
        }



        public async Task<Result<CartDto>> UpdateItemAsync(string userId, UpdateCartItemDto dto, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetActiveCartWithItemsTrackedAsync(userId, ct);
            if (cart is null)
                return Result.Failure<CartDto>(UserErrors.CartNotFound);

            var item = cart.Items.FirstOrDefault(i => i.Id == dto.CartItemId);
            if (item is null)
                return Result.Failure<CartDto>(UserErrors.CartItemNotFound);

            if (dto.Quantity <= 0)
                return Result.Failure<CartDto>(UserErrors.CartItemQuantityLessThanZero);

            item.Quantity = dto.Quantity;

            await _unitOfWork.SaveChangesAsync(ct);

            await _redis.DeleteAsync($"cart:{userId}");

            var updated = await _unitOfWork.Carts.GetActiveCartWithItemsAsync(userId, ct);
            var result = MapToDto(updated!);
            await _redis.SetAsync($"cart:{userId}", result);

            return Result<CartDto>.Success(result);
        }


        public async Task<Result<bool>> RemoveItemAsync(string userId, int cartItemId, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetActiveCartWithItemsTrackedAsync(userId, ct);
            if (cart is null)
                return Result.Failure<bool>(UserErrors.CartNotFound);

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item is null)
                return Result.Failure<bool>(UserErrors.CartItemNotFound);

            // Uses soft-delete from your generic repo if CartItem has IsDeleted,
            // otherwise hard-deletes
            // Better: remove via context directly since CartItem isn't its own repo
            cart.Items.Remove(item);

            await _unitOfWork.SaveChangesAsync(ct);

            await _redis.DeleteAsync($"cart:{userId}");
            return Result<bool>.Success(true);
        }

        public async Task<Result> ClearCartAsync(string userId, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetActiveCartWithItemsTrackedAsync(userId, ct);
            if (cart is null)
                return Result.Failure<bool>(UserErrors.CartNotFound);

            cart.Items.Clear();

            await _unitOfWork.SaveChangesAsync(ct);

            await _redis.DeleteAsync($"cart:{userId}");
            return Result.Success();
        }


        private static CartDto MapToDto(Cart cart) => new(
            cart.Id,
            cart.Status,
            cart.Items.Select(i => new CartItemDto(
                i.Id,
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice
            )).ToList(),
            cart.Items.Sum(i => i.Quantity * i.UnitPrice)
        );

    }
}
