namespace FastPay.Domain.Common;

public abstract class Entity<TKey>
{
    public TKey Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}