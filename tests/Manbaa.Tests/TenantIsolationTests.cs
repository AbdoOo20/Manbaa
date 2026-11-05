using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Manbaa.Application.Exceptions;
using Manbaa.Domain.Entities;
using Manbaa.Infrastructure.Persistence;
using Manbaa.Tests.Support;

namespace Manbaa.Tests;

public class TenantIsolationTests
{
    private static async Task<(int TeacherA, int TeacherB, int PlanId)> SeedAsync(TestApp app)
    {
        app.User.AsAdmin();

        return await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            var plan = new Plan { Name = "أساسي", MonthlyPrice = 100, MaxStudents = 100 };

            var teacherA = new Teacher { FullName = "أحمد", Subject = "فيزياء", Phone = "0100" };

            var teacherB = new Teacher { FullName = "محمود", Subject = "كيمياء", Phone = "0111" };

            context.AddRange(plan, teacherA, teacherB);

            await context.SaveChangesAsync();

            context.AddRange(
                new TeacherSubscription { TeacherId = teacherA.Id, PlanId = plan.Id, Months = 1, Amount = 100 },
                new TeacherSubscription { TeacherId = teacherB.Id, PlanId = plan.Id, Months = 1, Amount = 100 });

            await context.SaveChangesAsync();

            return (teacherA.Id, teacherB.Id, plan.Id);
        });
    }

    [Fact]
    public async Task A_teacher_only_sees_their_own_rows_and_the_admin_sees_all()
    {
        await using var app = await TestApp.StartAsync();

        var (teacherA, _, _) = await SeedAsync(app);

        app.User.AsTeacher(teacherA, userId: 50);

        var seenByTeacher = await app.RunAsync(services => services.GetRequiredService<ManbaaDbContext>().TeacherSubscriptions.Select(item => item.TeacherId).ToListAsync());

        Assert.Equal([teacherA], seenByTeacher);

        app.User.AsAdmin();

        var seenByAdmin = await app.RunAsync(services => services.GetRequiredService<ManbaaDbContext>().TeacherSubscriptions.CountAsync());

        Assert.Equal(2, seenByAdmin);
    }

    [Fact]
    public async Task A_teacher_cannot_write_another_teachers_row_and_new_rows_get_their_teacher()
    {
        await using var app = await TestApp.StartAsync();

        var (teacherA, teacherB, planId) = await SeedAsync(app);

        app.User.AsTeacher(teacherA, userId: 50);

        await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            context.TeacherSubscriptions.Add(new TeacherSubscription { TeacherId = teacherB, PlanId = planId, Months = 1 });

            await Assert.ThrowsAsync<ForbiddenException>(() => context.SaveChangesAsync());
        });

        var stamped = await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            var subscription = new TeacherSubscription { PlanId = planId, Months = 1 };

            context.TeacherSubscriptions.Add(subscription);

            await context.SaveChangesAsync();

            return subscription.TeacherId;
        });

        Assert.Equal(teacherA, stamped);
    }

    [Fact]
    public async Task Soft_deleted_rows_disappear_from_queries_but_stay_in_the_table()
    {
        await using var app = await TestApp.StartAsync();

        await SeedAsync(app);

        await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            context.Plans.Remove(await context.Plans.FirstAsync());

            await context.SaveChangesAsync();
        });

        var (visible, stored) = await app.RunAsync(async services =>
        {
            var context = services.GetRequiredService<ManbaaDbContext>();

            return (await context.Plans.CountAsync(), await context.Plans.IgnoreQueryFilters([ManbaaDbContext.SoftDeleteFilter]).CountAsync());
        });

        Assert.Equal(0, visible);

        Assert.Equal(1, stored);
    }
}
