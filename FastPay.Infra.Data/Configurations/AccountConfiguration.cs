using FastPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastPay.Infra.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.ClientId)
            .IsRequired();

        builder.OwnsOne(a => a.AvailableBalance, m =>
        {
            m.Property(p => p.Amount)
                .HasColumnName("available_balance")
                .HasPrecision(18, 2);
            m.Property(p => p.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(a => a.ReservedBalance, m =>
        {
            m.Property(p => p.Amount)
                .HasColumnName("reserved_balance")
                .HasPrecision(18, 2);
        });

        builder.OwnsOne(a => a.CreditLimit, m =>
        {
            m.Property(p => p.Amount)
                .HasColumnName("credit_limit")
                .HasPrecision(18, 2);
        });

        builder.Property(a => a.Status)
            .HasConversion<int>();

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamp with time zone");
    }
}
