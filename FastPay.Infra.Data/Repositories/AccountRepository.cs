using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using FastPay.Infra.Data.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task AddAsync(
        Account account, 
        CancellationToken cancellationToken)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts
            .FromSqlInterpolated($"SELECT * FROM accounts WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Account>> GetByIdsForUpdateAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken)
    {
        var idList = ids.Distinct().OrderBy(x => x).ToArray();
        if (idList.Length == 0) return new List<Account>();
        return await _dbContext.Accounts
            .FromSqlInterpolated($"SELECT * FROM accounts WHERE id = ANY ({idList}) FOR UPDATE")
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByClientIdAsync(
        string clientId, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.ClientId == clientId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetByClientAndCurrencyAsync(
        string clientId, 
        string currency, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ClientId == clientId && a.Currency == currency, 
            cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> ListAsync(
        string? clientId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? clientId, 
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Accounts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(a => a.ClientId == clientId);
        return await query.CountAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Account account, 
        CancellationToken cancellationToken)
    {
        _dbContext.Accounts.Update(account);
        await Task.CompletedTask;
    }
}
