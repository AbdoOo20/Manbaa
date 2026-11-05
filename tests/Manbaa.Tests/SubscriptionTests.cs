using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Manbaa.Application.Abstractions;
using Manbaa.Application.Common;
using Manbaa.Application.DTOs.Plans;
using Manbaa.Application.DTOs.Subscriptions;
using Manbaa.Application.DTOs.Teachers;
using Manbaa.Application.Services.Plans;
using Manbaa.Application.Services.Subscriptions;
using Manbaa.Application.Services.Teachers;
using Manbaa.Application.Settings;
using Manbaa.Domain.Enums;
using Manbaa.Infrastructure.Persistence;
using Manbaa.Tests.Support;

namespace Manbaa.Tests;

public class SubscriptionTests
{
    private static async Task<(int TeacherId, int PlanId)> CreateTeacherAsync(TestApp app)
    {
        app.User.AsAdmin();

        var plan = await app.RunAsync(services => services.GetRequiredService<IPlanService>().CreateAsync(new SavePlanRequest("أساسي", null, 200, 300, 1, 20, true, 1)));

        var teacher = await app.RunAsync(services => services.GetRequiredService<ITeacherAccountService>()
            .CreateAsync(new CreateTeacherRequest("أحمد علي", "فيزياء", "01000000001", "ahmed@example.com", "password123", plan.Data!.Id, null)));

        Assert.True(teacher.Succeeded, string.Join(", ", teacher.Errors));

        return (teacher.Data!.Id, plan.Data!.Id);
    }

    [Fact]
    public async Task A_new_teacher_starts_on_a_trial_with_a_login()
    {
        await using var app = await TestApp.StartAsync();

        var (teacherId, _) = await CreateTeacherAsync(app);

        var teacher = await app.RunAsync(services => services.GetRequiredService<ITeacherService>().GetByIdAsync(teacherId));

        Assert.Equal(TeacherStatus.Trial, teacher.Data!.Status);

        Assert.Equal(SubscriptionState.Trial, teacher.Data.State);

        Assert.Equal(app.Clock.Now.AddDays(14), teacher.Data.SubscriptionEndsAt);

        var userCount = await app.RunAsync(services => services.GetRequiredService<ManbaaDbContext>().Users.CountAsync(user => user.TeacherId == teacherId));

        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task Paying_activates_the_account_and_a_second_payment_continues_after_the_first()
    {
        await using var app = await TestApp.StartAsync();

        var (teacherId, planId) = await CreateTeacherAsync(app);

        var first = await app.RunAsync(services => services.GetRequiredService<ISubscriptionService>().RecordAsync(teacherId, new RecordSubscriptionRequest(planId, 1, null, null)));

        Assert.Equal(ResultStatus.Created, first.Status);

        Assert.Equal(app.Clock.Now, first.Data!.StartsAt);

        Assert.Equal(200, first.Data.Amount);

        Assert.Equal("S-000001", first.Data.ReceiptNumber);

        var second = await app.RunAsync(services => services.GetRequiredService<ISubscriptionService>().RecordAsync(teacherId, new RecordSubscriptionRequest(planId, 3, 500, "خصم")));

        Assert.Equal(first.Data.EndsAt, second.Data!.StartsAt);

        Assert.Equal(first.Data.EndsAt.AddMonths(3), second.Data.EndsAt);

        var teacher = await app.RunAsync(services => services.GetRequiredService<ITeacherService>().GetByIdAsync(teacherId));

        Assert.Equal(TeacherStatus.Active, teacher.Data!.Status);

        Assert.Equal(second.Data.EndsAt, teacher.Data.SubscriptionEndsAt);

        Assert.Contains(app.Publisher.Pushed, push => push.Notification.Type == "SubscriptionRenewed");
    }

    [Fact]
    public async Task Only_the_latest_payment_can_be_cancelled_and_the_end_date_moves_back()
    {
        await using var app = await TestApp.StartAsync();

        var (teacherId, planId) = await CreateTeacherAsync(app);

        var first = await app.RunAsync(services => services.GetRequiredService<ISubscriptionService>().RecordAsync(teacherId, new RecordSubscriptionRequest(planId, 1, null, null)));

        var second = await app.RunAsync(services => services.GetRequiredService<ISubscriptionService>().RecordAsync(teacherId, new RecordSubscriptionRequest(planId, 1, null, null)));

        var tooEarly = await app.RunAsync(services => services.GetRequiredService<ISubscriptionService>().CancelAsync(first.Data!.Id, new CancelSubscriptionRequest("غلط")));

        Assert.Equal(ResultStatus.Conflict, tooEarly.Status);

        var cancelled = await app.RunAsync(services => services.GetRequiredService<ISubscriptionService>().CancelAsync(second.Data!.Id, new CancelSubscriptionRequest("اتسجل مرتين")));

        Assert.True(cancelled.Data!.IsCancelled);

        var teacher = await app.RunAsync(services => services.GetRequiredService<ITeacherService>().GetByIdAsync(teacherId));

        Assert.Equal(first.Data!.EndsAt, teacher.Data!.SubscriptionEndsAt);

        Assert.Equal(2, teacher.Data.Subscriptions.Count);
    }

    [Fact]
    public async Task Suspending_or_running_out_locks_the_teacher_out()
    {
        await using var app = await TestApp.StartAsync();

        var (teacherId, _) = await CreateTeacherAsync(app);

        var access = await app.RunAsync(services => Task.FromResult(services.GetRequiredService<ITeacherAccessService>()));

        Assert.True(await access.IsActiveAsync(teacherId));

        app.Clock.Now = app.Clock.Now.AddDays(14 + 3 + 1);

        access.Forget(teacherId);

        Assert.False(await access.IsActiveAsync(teacherId));

        app.Clock.Now = app.Clock.Now.AddDays(-(14 + 3 + 1));

        await app.RunAsync(services => services.GetRequiredService<ITeacherService>().SuspendAsync(teacherId, new SuspendTeacherRequest("لم يدفع")));

        Assert.False(await access.IsActiveAsync(teacherId));
    }

    [Fact]
    public async Task The_expiry_reminder_is_sent_once_to_the_teacher_and_the_admins()
    {
        await using var app = await TestApp.StartAsync();

        var (teacherId, _) = await CreateTeacherAsync(app);

        app.Clock.Now = app.Clock.Now.AddDays(10);

        app.User.AsSystemJob();

        var first = await app.RunAsync(services => services.GetRequiredService<ISubscriptionReminderService>().SendExpiryRemindersAsync());

        var second = await app.RunAsync(services => services.GetRequiredService<ISubscriptionReminderService>().SendExpiryRemindersAsync());

        Assert.Equal(1, first);

        Assert.Equal(0, second);

        var reminders = await app.RunAsync(services => services.GetRequiredService<ManbaaDbContext>().Notifications
            .Where(notification => notification.TeacherId == teacherId && notification.Type == "SubscriptionExpiringSoon")
            .CountAsync());

        Assert.Equal(1, reminders);
    }

    [Theory]
    [InlineData(TeacherStatus.Trial, 5, SubscriptionState.Trial)]
    [InlineData(TeacherStatus.Active, 30, SubscriptionState.Active)]
    [InlineData(TeacherStatus.Active, 7, SubscriptionState.ExpiringSoon)]
    [InlineData(TeacherStatus.Active, -2, SubscriptionState.InGrace)]
    [InlineData(TeacherStatus.Active, -4, SubscriptionState.Expired)]
    [InlineData(TeacherStatus.Suspended, 30, SubscriptionState.Suspended)]
    public void The_state_follows_the_status_and_the_days_left(TeacherStatus status, int daysLeft, SubscriptionState expected)
    {
        var now = new DateTime(2026, 11, 5, 10, 0, 0, DateTimeKind.Utc);

        Assert.Equal(expected, SubscriptionRules.StateOf(status, now.AddDays(daysLeft), now, new SubscriptionSettings()));
    }
}
