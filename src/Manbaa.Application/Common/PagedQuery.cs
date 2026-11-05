using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.Common;

public class PagedQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    [MaxLength(100)]
    public string? Search { get; set; }
}
