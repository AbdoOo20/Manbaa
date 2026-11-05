namespace Manbaa.Domain.Common;

public abstract class BaseEntity<TKey> : IAuditable, ISoftDeletable
{
    public TKey Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
}
