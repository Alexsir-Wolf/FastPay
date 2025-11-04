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
}
