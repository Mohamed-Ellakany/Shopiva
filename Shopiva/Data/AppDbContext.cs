namespace Shopiva.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);



            builder.Entity<Product>()
                    .HasOne(p => p.Seller)
                    .WithMany()
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Review>()
                   .HasIndex(r => new { r.ProductId, r.CustomerId })
                   .IsUnique();

            builder.Entity<Review>()
                    .HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                    .HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Cart>(e =>
            {
                e.HasKey(c => c.Id);
                e.HasOne(c => c.User)
                 .WithOne(u => u.Cart)
                 .HasForeignKey<Cart>(c => c.UserId);
            });

            builder.Entity<CartItem>(e =>
            {
                e.HasKey(ci => ci.Id);
                e.HasOne(ci => ci.Cart)
                 .WithMany(c => c.Items)
                 .HasForeignKey(ci => ci.CartId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<IdentityRole>().HasData(
                  new IdentityRole
                  {
                      Id = "a1b2c3d4-0001-0000-0000-000000000001",
                      Name = "Admin",
                      NormalizedName = "ADMIN",
                      ConcurrencyStamp = "a1b2c3d4-0001-0000-0000-000000000011"
                  },
                   new IdentityRole
                   {
                       Id = "a1b2c3d4-0002-0000-0000-000000000002",
                       Name = "Customer",
                       NormalizedName = "CUSTOMER",
                       ConcurrencyStamp = "a1b2c3d4-0002-0000-0000-000000000022"
                   },
                   new IdentityRole
                   {
                       Id = "a1b2c3d4-0003-0000-0000-000000000003",
                       Name = "Seller",
                       NormalizedName = "SELLER",
                       ConcurrencyStamp = "a1b2c3d4-0003-0000-0000-000000000033"
                   }
               );

        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

    }
}
