using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence.Configurations;

internal class CacheEntityConfiguration : IEntityTypeConfiguration<CacheEntity>
{
    public void Configure(EntityTypeBuilder<CacheEntity> builder)
    {
        builder.ToTable("cache");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.CacheKey)
            .HasColumnName("cache_key")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.InputJson)
            .HasColumnName("input_json")
            .HasColumnType("jsonb");

        builder.Property(e => e.ResultJson)
            .HasColumnName("result_json")
            .HasColumnType("jsonb");

        builder.Property(e => e.ResultByte)
            .HasColumnName("result_byte");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.CacheKey)
            .IsUnique();

        builder.HasIndex(e => e.CreatedAt);
    }
}
