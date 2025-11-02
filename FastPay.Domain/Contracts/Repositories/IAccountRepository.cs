using FastPay.Domain.Entities;

namespace FastPay.Domain.Contracts.Repositories;

public interface IAccountRepository
{
    IUnitOfWork UnitOfWork { get; }

    Task<Account?> GetByIdAsync(int id);
    Task<IEnumerable<Account?>> GetByClientIdAsync(string clientId);
    Task<Account?> GetByClientAndCurrencyAsync(string clientId, string currency);
    Task<IReadOnlyList<Account>> ListAsync(string? clientId, int page, int pageSize);
    Task<int> CountAsync(string? clientId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}

