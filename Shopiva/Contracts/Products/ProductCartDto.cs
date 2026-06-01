namespace Shopiva.Contracts.Products
{
    public record ProductCartDto(
        int Id,
        string Name,
        decimal Price,
        decimal? DiscountPrice,
        ProductImage? ImageUrl
    );
}
