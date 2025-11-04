using FastPay.Domain.Entities;

namespace FastPay.Domain.Contracts.Repositories;

public interface ITransactionRepository
{
    IUnitOfWork UnitOfWork { get; }

    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<Transaction?> GetByReferenceAsync(int accountId, string referenceId, string operation, CancellationToken cancellationToken);
}
