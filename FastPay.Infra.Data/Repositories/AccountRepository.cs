using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using FastPay.Infra.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace FastPay.Infra.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly FastPayDbContext _dbContext;
    public IUnitOfWork UnitOfWork { get; }

    public AccountRepository(FastPayDbContext context)
    {
        _dbContext = context;
        UnitOfWork = new UnitOfWork(_dbContext);
    }

    public async Task AddAsync(Account account)
    {
        await _dbContext.Accounts.AddAsync(account);
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByClientIdAsync(string clientId)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ClientId == clientId);
    }

    public async Task<Account?> GetByClientAndCurrencyAsync(string clientId, string currency)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ClientId == clientId && a.Currency == currency);
    }

    public async Task UpdateAsync(Account account)
    {
        _dbContext.Accounts.Update(account);
        await Task.CompletedTask;
    }
}
