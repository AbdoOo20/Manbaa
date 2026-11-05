using Microsoft.AspNetCore.Identity;

using Manbaa.Domain.Common;

namespace Manbaa.Domain.Entities;

public class User : IdentityUser<int>, IAuditable, ISoftDeletable
{
    /// <summary>The teacher (tenant) the user belongs to. Null only for system admins.</summary>
    public int? TeacherId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Teacher? Teacher { get; set; }
}
