using ECommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceInventory.Infrastructure.Data.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Payload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Status);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.RetryCount)
            .IsRequired();
    }
}
