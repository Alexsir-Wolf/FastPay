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

    public async Task<IReadOnlyList<Account>> ListAsync(string? clientId, int page, int pageSize)
    {
        var query = _dbContext.Accounts.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(a => a.ClientId == clientId);

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        return await query
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? clientId)
    {
        var query = _dbContext.Accounts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(a => a.ClientId == clientId);
        return await query.CountAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        _dbContext.Accounts.Update(account);
        await Task.CompletedTask;
    }
}
