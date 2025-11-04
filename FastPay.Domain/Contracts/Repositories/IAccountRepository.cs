using FastPay.Domain.Entities;

namespace FastPay.Domain.Contracts.Repositories;

public interface IAccountRepository
{
    IUnitOfWork UnitOfWork { get; }

    Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Account?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<List<Account>> GetByIdsForUpdateAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    Task<IEnumerable<Account?>> GetByClientIdAsync(string clientId, CancellationToken cancellationToken);
    Task<Account?> GetByClientAndCurrencyAsync(string clientId, string currency, CancellationToken cancellationToken);
    Task<IReadOnlyList<Account>> ListAsync(string? clientId, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountAsync(string? clientId, CancellationToken cancellationToken);
    Task AddAsync(Account account, CancellationToken cancellationToken);
    Task UpdateAsync(Account account, CancellationToken cancellationToken);
}

