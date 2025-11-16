using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence.Configurations;

internal class HistoryEntryEntityConfiguration : IEntityTypeConfiguration<HistoryEntryEntity>
{
    public void Configure(EntityTypeBuilder<HistoryEntryEntity> builder)
    {
        builder.ToTable("history_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.CommandType)
            .HasColumnName("command_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.InputText)
            .HasColumnName("input_text")
            .HasMaxLength(6000)
            .IsRequired();

        builder.Property(e => e.OutputText)
            .HasColumnName("output_text")
            .HasMaxLength(6000)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.CreatedAt);
    }
}
