using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopiva.Config
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Description).IsRequired().HasMaxLength(500);
            builder.Property(p => p.Price).IsRequired();
            builder.Property(p => p.DiscountedPrice).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Stock).IsRequired();


        }
    }
}
