using FastPay.Domain.Contracts.Repositories;
using FastPay.Infra.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Serializable,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync<object>(async ct => { await action(ct); return new object(); }, isolationLevel, maxRetries, cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Serializable,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
                var result = await action(cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return result;
            }
            catch (DbUpdateException ex) when (IsRetryable(ex) && attempt++ < maxRetries)
            {
                await Task.Delay(Backoff(attempt), cancellationToken);
                continue;
            }
            catch (PostgresException ex) when (IsRetryable(ex) && attempt++ < maxRetries)
            {
                await Task.Delay(Backoff(attempt), cancellationToken);
                continue;
            }
        }
    }

    private static bool IsRetryable(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pex)
            return IsRetryable(pex);
        return false;
    }

    private static bool IsRetryable(PostgresException ex)
    {
        return ex.SqlState == PostgresErrorCodes.SerializationFailure || ex.SqlState == PostgresErrorCodes.DeadlockDetected;
    }

    private static TimeSpan Backoff(int attempt)
    {
        var baseMs = 100 * Math.Pow(2, attempt - 1);
        var jitter = Random.Shared.Next(0, 100);
        return TimeSpan.FromMilliseconds(baseMs + jitter);
    }
}
