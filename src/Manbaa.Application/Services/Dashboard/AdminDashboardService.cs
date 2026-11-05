using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Dashboard;

using Manbaa.Application.Mappings;

using Manbaa.Application.Services.Subscriptions;

using Manbaa.Application.Settings;

using Manbaa.Domain.Entities;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.Services.Dashboard;

public class AdminDashboardService(IUnitOfWork unitOfWork, IOptions<SubscriptionSettings> settingsOptions, TimeProvider clock) : IAdminDashboardService
{
    private const int RevenueMonths = 12;

    private const int ExpiringListSize = 10;

    private readonly SubscriptionSettings settings = settingsOptions.Value;

    public async Task<Result<AdminDashboardDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var thisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var lastMonth = thisMonth.AddMonths(-1);

        var firstRevenueMonth = thisMonth.AddMonths(-(RevenueMonths - 1));

        var teachers = unitOfWork.Repository<Teacher>().Query();

        var counts = new Dictionary<SubscriptionState, int>();

        foreach (var state in Enum.GetValues<SubscriptionState>())
        {
            counts[state] = await teachers.WhereState(state, now, settings).CountAsync(cancellationToken);
        }

        var total = await teachers.CountAsync(cancellationToken);

        var newThisMonth = await teachers.CountAsync(teacher => teacher.CreatedAt >= thisMonth, cancellationToken);

        var payments = await unitOfWork.Repository<TeacherSubscription>().Query()
            .Where(subscription => !subscription.IsCancelled && subscription.CreatedAt >= firstRevenueMonth)
            .GroupBy(subscription => new { subscription.CreatedAt.Year, subscription.CreatedAt.Month })
            .Select(group => new MonthlyRevenueDto(group.Key.Year, group.Key.Month, group.Sum(subscription => subscription.Amount), group.Count()))
            .ToListAsync(cancellationToken);

        var revenueByMonth = Enumerable.Range(0, RevenueMonths)
            .Select(offset => firstRevenueMonth.AddMonths(offset))
            .Select(month => payments.FirstOrDefault(item => item.Year == month.Year && item.Month == month.Month) ?? new MonthlyRevenueDto(month.Year, month.Month, 0, 0))
            .ToList();

        var expiringRows = await teachers
            .WhereState(SubscriptionState.ExpiringSoon, now, settings)
            .OrderBy(teacher => teacher.SubscriptionEndsAt)
            .Take(ExpiringListSize)
            .SelectRows()
            .ToListAsync(cancellationToken);

        return Result<AdminDashboardDto>.Ok(new AdminDashboardDto(
            total,
            counts[SubscriptionState.Trial],
            counts[SubscriptionState.Active],
            counts[SubscriptionState.ExpiringSoon],
            counts[SubscriptionState.InGrace],
            counts[SubscriptionState.Expired],
            counts[SubscriptionState.Suspended],
            newThisMonth,
            revenueByMonth[^1].Amount,
            revenueByMonth[^2].Amount,
            revenueByMonth,
            expiringRows.Select(row => row.ToListItem(now, settings)).ToList()));
    }
}
