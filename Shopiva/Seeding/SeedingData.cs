using Shopiva.Models;

namespace Shopiva.Seeding
{
    /// <summary>
    /// Helper class for generating seed data.
    /// Used in OnModelCreating to seed the database using EF Core HasData().
    /// All values are hardcoded (static) to avoid EF Core migration warnings.
    /// </summary>
    public static class SeedingData
    {
        private static readonly DateTime SeedDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        // Pre-defined seller user IDs (will be created by SeedUsers.cs)
        private static readonly string Seller1_UserId = "fatima.ali"; // Will be assigned a GUID by UserManager
        private static readonly string Seller2_UserId = "mohamed.ibrahim";
        private static readonly string Seller3_UserId = "omar.ahmed";

        /// <summary>
        /// Gets seed categories data with hardcoded static values
        /// </summary>
        public static List<Category> GetCategories()
        {
            var categories = new List<Category>
            {
                new()
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and gadgets",
                    ImageUrl = "/uploads/categories/electronics.jpg",
                    CreatedAt = SeedDate
                },
                new()
                {
                    Id = 2,
                    Name = "Clothing",
                    Description = "Fashion and apparel for all ages",
                    ImageUrl = "/uploads/categories/clothes.jpg",
                    CreatedAt = SeedDate
                },
                new()
                {
                    Id = 3,
                    Name = "Accessories",
                    Description = "Stylish accessories for every occasion",
                    ImageUrl = "/uploads/categories/accessories.jpg",
                    CreatedAt = SeedDate
                },
                new()
                {
                    Id = 4,
                    Name = "Toys",
                    Description = "Fun and educational toys for children",
                    ImageUrl = "/uploads/categories/Toys.jpg",
                    CreatedAt = SeedDate
                }
            };

            return categories;
        }

        /// <summary>
        /// Gets seed products data with hardcoded static values
        /// Note: Seller IDs need to be resolved from database after users are created
        /// </summary>
        public static List<(string sellerUsername, int categoryId, string name, string description, decimal price, decimal? discountedPrice, int stock)> GetProductsData()
        {
            return new List<(string, int, string, string, decimal, decimal?, int)>
            {
                // Electronics Category
                ("fatima.ali", 1, "Smart Watch - Black", "Advanced smartwatch with fitness tracking, heart rate monitor, and 7-day battery life. Compatible with iOS and Android.", 249.99m, 199.99m, 50),
                ("fatima.ali", 1, "Smart Watch - White", "Premium smartwatch in elegant white color with AMOLED display, water-resistant design, and health monitoring features.", 249.99m, null, 35),
                ("mohamed.ibrahim", 1, "DSLR Camera", "Professional DSLR camera with 24.2MP sensor, 4K video recording, and advanced autofocus system. Perfect for photography enthusiasts.", 899.99m, 799.99m, 20),
                ("mohamed.ibrahim", 1, "Polaroid Camera", "Retro instant camera that produces physical prints. Great for capturing memories and creating instant scrapbooks.", 99.99m, 79.99m, 40),
                ("omar.ahmed", 1, "Wireless Headphones - Black", "Premium wireless headphones with noise cancellation, 30-hour battery life, and superior sound quality.", 349.99m, 279.99m, 60),

                // Clothing Category
                ("fatima.ali", 2, "Sneaker - Black", "Comfortable and stylish black sneakers perfect for daily wear, sports, and casual outings. Breathable mesh design.", 89.99m, 69.99m, 100),
                ("fatima.ali", 2, "Nike Red Sneaker", "Iconic Nike sneaker in bold red color with premium cushioning and iconic swoosh design. Limited edition.", 129.99m, 99.99m, 75),
                ("mohamed.ibrahim", 2, "High Heels - Floral", "Elegant high heels with beautiful floral pattern. Perfect for parties, weddings, and special occasions.", 119.99m, null, 45),

                // Accessories Category
                ("mohamed.ibrahim", 3, "Sunglasses - Black", "Classic black sunglasses with UV400 protection, polarized lenses, and comfortable fit for all-day wear.", 79.99m, 59.99m, 80),
                ("omar.ahmed", 3, "Sunglasses - Gold", "Luxury sunglasses with gold-tinted frame and premium glass lenses. Make a stylish statement.", 99.99m, 79.99m, 55),
                ("omar.ahmed", 3, "Vintage Watch", "Elegant vintage-style watch with leather strap, mechanical movement, and timeless design.", 149.99m, 119.99m, 30),
                ("fatima.ali", 3, "Water Bottle - Green", "Eco-friendly water bottle with insulated design, keeps drinks cold for 24 hours, leak-proof cap.", 34.99m, 24.99m, 150),

                // Toys Category
                ("fatima.ali", 4, "Toy Car - Yellow", "Bright yellow toy car with smooth wheels, perfect for young children. Durable and safe toy.", 19.99m, 14.99m, 200),
                ("mohamed.ibrahim", 4, "Bicycle - Black", "Children's bicycle in black color with training wheels, safety features, and comfortable seat.", 199.99m, 159.99m, 25),

                // Skincare Products
                ("omar.ahmed", 3, "Skincare Bottles Set", "Complete skincare routine set with cleanser, toner, moisturizer, and serum in elegant bottles.", 79.99m, 59.99m, 90),
                ("mohamed.ibrahim", 3, "Skincare Tubes - Orange", "Natural skincare products in eco-friendly tubes. Organic ingredients for healthy and radiant skin.", 49.99m, 39.99m, 120)
            };
        }

        /// <summary>
        /// Gets product images data with relative paths
        /// </summary>
        public static List<(string productName, string imagePath, bool isMain)> GetProductImagesData()
        {
            return new List<(string, string, bool)>
            {
                // Electronics
                ("Smart Watch - Black", "/uploads/products/smartwatch-black.jpg", true),
                ("Smart Watch - White", "/uploads/products/smartwatch-white.jpg", true),
                ("DSLR Camera", "/uploads/products/camera-dslr.jpg", true),
                ("Polaroid Camera", "/uploads/products/camera-polaroid.jpg", true),
                ("Wireless Headphones - Black", "/uploads/products/headphones-black-yellow.jpg", true),

                // Clothing
                ("Sneaker - Black", "/uploads/products/sneaker-black.jpg", true),
                ("Nike Red Sneaker", "/uploads/products/sneaker-nike-red.jpg", true),
                ("High Heels - Floral", "/uploads/products/high-heels-floral.jpg", true),

                // Accessories
                ("Sunglasses - Black", "/uploads/products/sunglasses-black.jpg", true),
                ("Sunglasses - Gold", "/uploads/products/sunglasses-gold.jpg", true),
                ("Vintage Watch", "/uploads/products/watch-vintage.jpg", true),
                ("Water Bottle - Green", "/uploads/products/water-bottle-green.jpg", true),

                // Toys
                ("Toy Car - Yellow", "/uploads/products/car-toy-yellow.jpg", true),
                ("Bicycle - Black", "/uploads/products/bicycle-black.jpg", true),

                // Skincare
                ("Skincare Bottles Set", "/uploads/products/skincare-bottles-stacked.jpg", true),
                ("Skincare Tubes - Orange", "/uploads/products/skincare-tubes-orange.jpg", true)
            };
        }
    }
}
