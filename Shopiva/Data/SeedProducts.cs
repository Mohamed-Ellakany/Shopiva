using Shopiva.Models;
using Shopiva.Seeding;

namespace Shopiva.Data
{
    public static class SeedProducts
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, AppDbContext dbContext)
            {
                // Check if products already exist
                if (dbContext.Products.Any())
                {
                    return;
                }

                var productsData = SeedingData.GetProductsData();

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
                        CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                    };

                    dbContext.Products.Add(product);
                }

                await dbContext.SaveChangesAsync();
            }
    }
}
