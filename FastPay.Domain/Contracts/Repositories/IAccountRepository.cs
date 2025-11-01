using FastPay.Domain.Entities;

namespace FastPay.Domain.Contracts.Repositories;

public interface IAccountRepository
{
    IUnitOfWork UnitOfWork { get; }

    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByClientIdAsync(int clientId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}

