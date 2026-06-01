using Shopiva.Abstractions.enums;

namespace Shopiva.Contracts.Cart
{
    public record AddToCartDto(int ProductId, int Quantity, decimal UnitPrice);
    public record UpdateCartItemDto(int CartItemId, int Quantity);

    public record CartItemDto(
        int Id,
        ProductCartDto Product,
        int Quantity,
        decimal UnitPrice,
        decimal Subtotal
    );

    public record CartDto(
        int Id,
        CartStatus Status,
        List<CartItemDto> Items,
        decimal Total
    );
}
