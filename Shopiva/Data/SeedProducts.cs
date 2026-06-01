using Shopiva.Models;
using Shopiva.Seeding;

namespace Shopiva.Data
{
    public static class SeedProducts
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            // Check if products already exist
            if (dbContext.Products.Any())
            {
                return;
            }

            // Get base URL dynamically from current request
            var request = httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null 
                ? $"{request.Scheme}://{request.Host}" 
                : "https://localhost:7259"; // Fallback URL

            var productsData = SeedingData.GetProductsData();
            var productImagesData = SeedingData.GetProductImagesData();

            foreach (var productData in productsData)
            {
                // Find the seller user
                var seller = await userManager.FindByNameAsync(productData.sellerUsername);
                if (seller == null)
                {
                    continue; // Skip if seller not found
                }

                var product = new Product
                {
                    Name = productData.name,
                    Description = productData.description,
                    Price = productData.price,
                    DiscountedPrice = productData.discountedPrice,
                    Stock = productData.stock,
                    IsActive = true,
                    CategoryId = productData.categoryId,
                    SellerId = seller.Id,
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    Images = new List<ProductImage>()
                };

                // Add product images
                var relatedImages = productImagesData
                    .Where(img => img.productName == productData.name)
                    .ToList();

                foreach (var image in relatedImages)
                {
                    product.Images.Add(new ProductImage
                    {
                        Url = $"{baseUrl}{image.imagePath}",
                        IsMain = image.isMain
                    });
                }

                dbContext.Products.Add(product);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
