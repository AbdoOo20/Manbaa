using Manbaa.Application.Common;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.DTOs.Teachers;

public class TeacherQuery : PagedQuery
{
    public SubscriptionState? State { get; set; }

    public int? PlanId { get; set; }
}
