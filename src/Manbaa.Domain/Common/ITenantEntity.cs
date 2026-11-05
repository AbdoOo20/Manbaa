namespace Manbaa.Domain.Common;

/// <summary>
/// A row that belongs to one teacher (the tenant). Queries only return the current teacher's rows,
/// and saving a row of another teacher is refused.
/// </summary>
public interface ITenantEntity
{
    int TeacherId { get; set; }
}
