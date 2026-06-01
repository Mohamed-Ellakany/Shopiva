using Shopiva.Models;

namespace Shopiva.Data
{
    public static class SeedCategories
    {
        public static async Task SeedAsync(AppDbContext dbContext)
        {
            // Check if categories already exist
            if (dbContext.Categories.Any())
            {
                return;
            }

            var categories = new List<Category>
            {
                new()
                {
                    Name = "Electronics",
                    Description = "Electronic devices and gadgets",
                    ImageUrl = "/uploads/categories/electronics.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Name = "Clothing",
                    Description = "Fashion and apparel for all ages",
                    ImageUrl = "/uploads/categories/clothes.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Name = "Accessories",
                    Description = "Stylish accessories for every occasion",
                    ImageUrl = "/uploads/categories/accessories.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Name = "Toys",
                    Description = "Fun and educational toys for children",
                    ImageUrl = "/uploads/categories/Toys.jpg",
                    CreatedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.SaveChangesAsync();
        }
    }
}
