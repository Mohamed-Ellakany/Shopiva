using Shopiva.Models;

namespace Shopiva.Data
{
    public static class SeedCategories
    {
        public static async Task SeedAsync(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            // Check if categories already exist
            if (dbContext.Categories.Any())
            {
                return;
            }

            // Get base URL dynamically from current request
            var request = httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null 
                ? $"{request.Scheme}://{request.Host}" 
                : "https://localhost:7259"; 

            var categories = new List<Category>
            {
                new()
                {
                    Name = "Electronics",
                    Description = "Electronic devices and gadgets",
                    ImageUrl = $"{baseUrl}/uploads/categories/electronics.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Name = "Clothing",
                    Description = "Fashion and apparel for all ages",
                    ImageUrl = $"{baseUrl}/uploads/categories/clothes.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Name = "Accessories",
                    Description = "Stylish accessories for every occasion",
                    ImageUrl = $"{baseUrl}/uploads/categories/accessories.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Name = "Toys",
                    Description = "Fun and educational toys for children",
                    ImageUrl = $"{baseUrl}/uploads/categories/Toys.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.SaveChangesAsync();
        }
    }
}
