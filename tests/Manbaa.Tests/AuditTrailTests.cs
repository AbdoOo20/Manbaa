using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Manbaa.Application.DTOs.Plans;
using Manbaa.Application.Services.Plans;
using Manbaa.Domain.Constants;
using Manbaa.Domain.Entities;
using Manbaa.Infrastructure.Persistence;
using Manbaa.Tests.Support;

namespace Manbaa.Tests;

public class AuditTrailTests
{
    private static SavePlanRequest PlanRequest(decimal price) => new("الباقة الذهبية", null, price, 500, 2, 50, true, 1);

    [Fact]
    public async Task Create_update_and_delete_are_each_recorded_with_who_and_values()
    {
        await using var app = await TestApp.StartAsync();

        app.User.AsAdmin(userId: 7);

        var created = await app.RunAsync(services => services.GetRequiredService<IPlanService>().CreateAsync(PlanRequest(300)));

        var planId = created.Data!.Id;

        await app.RunAsync(services => services.GetRequiredService<IPlanService>().UpdateAsync(planId, PlanRequest(450)));

        await app.RunAsync(services => services.GetRequiredService<IPlanService>().DeleteAsync(planId));

        var logs = await app.RunAsync(services => services.GetRequiredService<ManbaaDbContext>().AuditLogs
            .Where(log => log.EntityName == nameof(Plan))
            .OrderBy(log => log.Id)
            .ToListAsync());

        Assert.Equal([AuditActions.Create, AuditActions.Update, AuditActions.Delete], logs.Select(log => log.Action));

        Assert.All(logs, log =>
        {
            Assert.Equal(7, log.UserId);

            Assert.Equal(RoleNames.SystemAdmin, log.UserRole);

            Assert.Equal("10.0.0.1", log.IpAddress);

            Assert.Equal(planId.ToString(), log.EntityId);
        });

        Assert.Contains("الباقة الذهبية", logs[0].NewValues);

        Assert.Contains("300", logs[1].OldValues);

        Assert.Contains("450", logs[1].NewValues);

        Assert.DoesNotContain("Name", logs[1].NewValues);
    }

    [Fact]
    public async Task Audit_rows_cannot_be_changed_or_deleted()
    {
        await using var app = await TestApp.StartAsync();

        app.User.AsAdmin();

        await app.RunAsync(services => services.GetRequiredService<IPlanService>().CreateAsync(PlanRequest(300)));

        await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            var log = await context.AuditLogs.FirstAsync();

            log.NewValues = "{}";

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        });

        await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            context.AuditLogs.Remove(await context.AuditLogs.FirstAsync());

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        });
    }

    [Fact]
    public async Task A_failed_save_writes_no_audit_rows()
    {
        await using var app = await TestApp.StartAsync();

        app.User.AsAdmin();

        await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            context.TeacherSubscriptions.Add(new TeacherSubscription { TeacherId = 999, PlanId = 999, Months = 1 });

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        });

        var count = await app.RunAsync(services => services.GetRequiredService<ManbaaDbContext>().AuditLogs.CountAsync(log => log.EntityName == nameof(TeacherSubscription)));

        Assert.Equal(0, count);
    }
}
