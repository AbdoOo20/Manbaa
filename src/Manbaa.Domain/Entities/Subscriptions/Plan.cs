using Manbaa.Domain.Common;

namespace Manbaa.Domain.Entities;

/// <summary>A subscription package the system admin sells to teachers, with its limits.</summary>
public class Plan : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal MonthlyPrice { get; set; }

    public int MaxStudents { get; set; }

    public int MaxSecretaries { get; set; }

    public int MaxStorageGb { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
