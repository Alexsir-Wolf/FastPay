using FastPay.Domain.Contracts.Repositories;
using FastPay.Infra.Data.Persistence;

namespace FastPay.Infra.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FastPayDbContext _dbContext;

    public UnitOfWork(FastPayDbContext context)
    {
        _dbContext = context;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}