namespace Shopiva.Contracts.Products
{
    public record ProductCartDto(
        int Id,
        string Name,
        decimal Price,
        int Stock,
        decimal? DiscountPrice,
        string? ImageUrl    
    );
}
