using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using FastPay.Infra.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FastPay.Infra.Data.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FastPayDbContext _dbContext;
    public IUnitOfWork UnitOfWork { get; }

    public TransactionRepository(FastPayDbContext context)
    {
        _dbContext = context;
        UnitOfWork = new UnitOfWork(_dbContext);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<Transaction?> GetByReferenceAsync(
        int accountId, 
        string referenceId, 
        string operation, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.AccountId == accountId 
                    && t.ReferenceId == referenceId 
                    && t.Operation == operation.ToLower(), 
                    cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> ListByAccountAsync(
        int accountId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByAccountAsync(
        int accountId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .CountAsync(cancellationToken);
    }
}
