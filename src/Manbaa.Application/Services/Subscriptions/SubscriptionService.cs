using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Subscriptions;

using Manbaa.Application.Mappings;

using Manbaa.Application.Services.Notifications;

using Manbaa.Application.Settings;

using Manbaa.Domain.Entities;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.Services.Subscriptions;

public class SubscriptionService(
    IUnitOfWork unitOfWork,
    IUserAppService currentUser,
    INotificationService notifications,
    INotificationRecipients recipients,
    ITeacherAccessService teacherAccess,
    IOptions<SubscriptionSettings> settingsOptions,
    TimeProvider clock) : ISubscriptionService
{
    private readonly SubscriptionSettings settings = settingsOptions.Value;

    public async Task<Result<PagedResult<SubscriptionDto>>> GetAllAsync(SubscriptionQuery query, CancellationToken cancellationToken = default)
    {
        var subscriptions = unitOfWork.Repository<TeacherSubscription>().Query();

        if (query.TeacherId is { } teacherId)
        {
            subscriptions = subscriptions.Where(subscription => subscription.TeacherId == teacherId);
        }

        if (!query.IncludeCancelled)
        {
            subscriptions = subscriptions.Where(subscription => !subscription.IsCancelled);
        }

        if (query.From is { } from)
        {
            subscriptions = subscriptions.Where(subscription => subscription.CreatedAt >= from);
        }

        if (query.To is { } to)
        {
            subscriptions = subscriptions.Where(subscription => subscription.CreatedAt < to.Date.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            subscriptions = subscriptions.Where(subscription => subscription.Teacher!.FullName.Contains(search) || subscription.Teacher.Phone.Contains(search));
        }

        var page = await subscriptions
            .OrderByDescending(subscription => subscription.Id)
            .Select(SubscriptionMappings.ToDto)
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<SubscriptionDto>>.Ok(page);
    }

    public async Task<Result<SubscriptionDto>> RecordAsync(int teacherId, RecordSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var teacher = await unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId, cancellationToken);

        if (teacher is null)
        {
            return Result<SubscriptionDto>.NotFound("المدرس مش موجود.");
        }

        var plan = await unitOfWork.Repository<Plan>().GetByIdAsync(request.PlanId, cancellationToken);

        if (plan is null || !plan.IsActive)
        {
            return Result<SubscriptionDto>.BadRequest("الباقة مش موجودة أو موقوفة.");
        }

        var now = clock.GetUtcNow().UtcDateTime;

        // A paid period continues right after the current one; a trial is replaced from today.
        var startsAt = teacher.Status != TeacherStatus.Trial && teacher.SubscriptionEndsAt > now ? teacher.SubscriptionEndsAt.Value : now;

        var subscription = new TeacherSubscription
        {
            TeacherId = teacher.Id,

            PlanId = plan.Id,

            Months = request.Months,

            Amount = request.Amount ?? plan.MonthlyPrice * request.Months,

            StartsAt = startsAt,

            EndsAt = startsAt.AddMonths(request.Months),

            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        await unitOfWork.Repository<TeacherSubscription>().AddAsync(subscription, cancellationToken);

        teacher.PlanId = plan.Id;

        teacher.SubscriptionEndsAt = subscription.EndsAt;

        teacher.ExpiryReminderSentAt = null;

        if (teacher.Status == TeacherStatus.Trial)
        {
            teacher.Status = TeacherStatus.Active;
        }

        await notifications.QueueAsync(
            await recipients.TeacherOwnersAsync(teacher.Id, cancellationToken),
            teacher.Id,
            NotificationTypes.SubscriptionRenewed,
            "تم تجديد الاشتراك",
            $"اشتراكك في باقة {plan.Name} شغال لحد {subscription.EndsAt:yyyy/MM/dd}.",
            "/subscription",
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        teacherAccess.Forget(teacher.Id);

        return Result<SubscriptionDto>.Created(await LoadAsync(subscription.Id, cancellationToken), "الاشتراك اتسجل.");
    }

    public async Task<Result<SubscriptionDto>> CancelAsync(int subscriptionId, CancelSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = await unitOfWork.Repository<TeacherSubscription>().GetByIdAsync(subscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result<SubscriptionDto>.NotFound("الاشتراك مش موجود.");
        }

        if (subscription.IsCancelled)
        {
            return Result<SubscriptionDto>.Conflict("الاشتراك ده ملغي بالفعل.");
        }

        var latestId = await unitOfWork.Repository<TeacherSubscription>().Query()
            .Where(item => item.TeacherId == subscription.TeacherId && !item.IsCancelled)
            .MaxAsync(item => (int?)item.Id, cancellationToken);

        if (latestId != subscription.Id)
        {
            return Result<SubscriptionDto>.Conflict("تقدر تلغي آخر اشتراك بس. ألغي الاشتراكات اللي بعده الأول.");
        }

        var teacher = await unitOfWork.Repository<Teacher>().GetByIdAsync(subscription.TeacherId, cancellationToken);

        if (teacher is null)
        {
            return Result<SubscriptionDto>.NotFound("المدرس مش موجود.");
        }

        subscription.IsCancelled = true;

        subscription.CancelReason = request.Reason.Trim();

        subscription.CancelledAt = clock.GetUtcNow().UtcDateTime;

        subscription.CancelledBy = currentUser.UserId;

        var previous = await unitOfWork.Repository<TeacherSubscription>().Query()
            .Where(item => item.TeacherId == teacher.Id && !item.IsCancelled && item.Id != subscription.Id)
            .OrderByDescending(item => item.Id)
            .Select(item => new { item.PlanId, item.EndsAt })
            .FirstOrDefaultAsync(cancellationToken);

        teacher.SubscriptionEndsAt = previous?.EndsAt ?? teacher.TrialEndsAt;

        teacher.PlanId = previous?.PlanId ?? teacher.PlanId;

        if (teacher.Status != TeacherStatus.Suspended)
        {
            teacher.Status = previous is null ? TeacherStatus.Trial : TeacherStatus.Active;
        }

        await notifications.QueueAsync(
            await recipients.TeacherOwnersAsync(teacher.Id, cancellationToken),
            teacher.Id,
            NotificationTypes.SubscriptionCancelled,
            "تم إلغاء اشتراك",
            $"اتلغى الإيصال {subscription.ReceiptNumber}. السبب: {subscription.CancelReason}",
            "/subscription",
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        teacherAccess.Forget(teacher.Id);

        return Result<SubscriptionDto>.Ok(await LoadAsync(subscription.Id, cancellationToken), "الاشتراك اتلغى.");
    }

    public async Task<Result<MySubscriptionDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var teacherId = currentUser.RequiredTeacherId;

        var teacher = await unitOfWork.Repository<Teacher>().Query(true, item => item.Plan!).FirstOrDefaultAsync(item => item.Id == teacherId, cancellationToken);

        if (teacher is null)
        {
            return Result<MySubscriptionDto>.NotFound("الحساب مش موجود.");
        }

        var history = await unitOfWork.Repository<TeacherSubscription>().Query()
            .Where(item => item.TeacherId == teacherId)
            .OrderByDescending(item => item.Id)
            .Select(SubscriptionMappings.ToDto)
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow().UtcDateTime;

        return Result<MySubscriptionDto>.Ok(new MySubscriptionDto(
            teacher.Plan?.Name,
            teacher.Plan?.MaxStudents,
            teacher.Plan?.MaxSecretaries,
            teacher.Plan?.MaxStorageGb,
            teacher.Status,
            SubscriptionRules.StateOf(teacher.Status, teacher.SubscriptionEndsAt, now, settings),
            teacher.SubscriptionEndsAt,
            SubscriptionRules.DaysLeft(teacher.SubscriptionEndsAt, now),
            history));
    }

    private Task<SubscriptionDto> LoadAsync(int id, CancellationToken cancellationToken) =>
        unitOfWork.Repository<TeacherSubscription>().Query().Where(item => item.Id == id).Select(SubscriptionMappings.ToDto).FirstAsync(cancellationToken);
}
