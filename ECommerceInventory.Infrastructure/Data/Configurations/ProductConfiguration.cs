using ECommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceInventory.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}
