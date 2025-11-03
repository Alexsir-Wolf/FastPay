using FastPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastPay.Infra.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.AccountId)
            .HasColumnName("account_id");

        builder.Property(t => t.DestinationAccountId)
            .HasColumnName("destination_account_id");

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3);

        builder.Property(t => t.Operation)
            .HasColumnName("operation")
            .HasConversion<int>();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(t => t.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(t => t.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(t => t.MetadataJson)
            .HasColumnName("metadata_json")
            .HasColumnType("jsonb");

        builder.Property(t => t.Timestamp)
            .HasColumnName("timestamp");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(t => new { t.AccountId, t.Operation, t.ReferenceId })
            .HasDatabaseName("ix_transactions_idempotency")
            .IsUnique();
    }
}
