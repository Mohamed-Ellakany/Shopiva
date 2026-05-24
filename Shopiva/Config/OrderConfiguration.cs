using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopiva.Config
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(o => o.OrderNumber).IsUnique();

            builder.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(300);
            builder.Property(o => o.ShippingCity).IsRequired().HasMaxLength(100);
            builder.Property(o => o.ShippingCountry).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Notes).HasMaxLength(500);

            builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            builder.Property(o => o.ShippingFee).HasColumnType("decimal(18,2)");

            // Total is computed — not mapped to a column
            builder.Ignore(o => o.Total);

            builder.HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.StatusHistory)
                .WithOne(h => h.Order)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");

            // Subtotal is computed — not stored
            builder.Ignore(i => i.Subtotal);

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Note).HasMaxLength(300);
        }
    }
}