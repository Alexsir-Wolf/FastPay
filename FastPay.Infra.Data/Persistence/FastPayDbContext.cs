using FastPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FastPay.Infra.Data.Persistence;

public class FastPayDbContext : DbContext
{
    public FastPayDbContext(DbContextOptions<FastPayDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FastPayDbContext).Assembly);
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
}