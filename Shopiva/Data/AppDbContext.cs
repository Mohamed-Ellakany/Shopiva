using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shopiva.Models;
using System.Reflection;
using System.Reflection.Emit;

namespace Shopiva.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);

            // ── Product ──
            builder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany()
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.DiscountedPrice)
                .HasPrecision(18, 2);

            // ── Review ──
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

            // ── Cart ──
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

                e.Property(ci => ci.UnitPrice)
                 .HasPrecision(18, 2);
            });

            // ── Order ──
            builder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasPrecision(18, 2);

            // ── Payment ──
            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // ── PromoCode ──
            builder.Entity<PromoCode>()
                .Property(p => p.DiscountPercent)
                .HasPrecision(5, 2);

            // ── Roles Seed ──
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
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<Banner> Banners { get; set; }

    }
}