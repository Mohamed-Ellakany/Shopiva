using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopiva.Config
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder
                .OwnsMany(x=>x.RefreshTokens)
                .ToTable("RefreshTokens")
                .WithOwner()
                .HasForeignKey("UserId");

            builder.Property(x=>x.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(x=>x.LastName).HasMaxLength(100).IsRequired();

        }
    }
}
