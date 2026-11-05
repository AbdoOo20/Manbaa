using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Teachers;

using Manbaa.Application.Mappings;

using Manbaa.Application.Services.Notifications;

using Manbaa.Application.Services.Subscriptions;

using Manbaa.Application.Settings;

using Manbaa.Domain.Entities;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.Services.Teachers;

public class TeacherService(
    IUnitOfWork unitOfWork,
    INotificationService notifications,
    INotificationRecipients recipients,
    ITeacherAccessService teacherAccess,
    IOptions<SubscriptionSettings> settingsOptions,
    TimeProvider clock) : ITeacherService
{
    private readonly SubscriptionSettings settings = settingsOptions.Value;

    public async Task<Result<PagedResult<TeacherListItemDto>>> GetAllAsync(TeacherQuery query, CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var teachers = unitOfWork.Repository<Teacher>().Query().WhereMatchesSearch(query.Search);

        if (query.State is { } state)
        {
            teachers = teachers.WhereState(state, now, settings);
        }

        if (query.PlanId is { } planId)
        {
            teachers = teachers.Where(teacher => teacher.PlanId == planId);
        }

        var page = await teachers
            .OrderByDescending(teacher => teacher.Id)
            .SelectRows()
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);

        var items = page.Items.Select(row => row.ToListItem(now, settings)).ToList();

        return Result<PagedResult<TeacherListItemDto>>.Ok(new PagedResult<TeacherListItemDto>(items, page.Page, page.PageSize, page.TotalCount));
    }

    public async Task<Result<TeacherDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var teacher = await unitOfWork.Repository<Teacher>().Query(true, item => item.Plan!).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (teacher is null)
        {
            return Result<TeacherDetailsDto>.NotFound("المدرس مش موجود.");
        }

        var subscriptions = await unitOfWork.Repository<TeacherSubscription>().Query()
            .Where(subscription => subscription.TeacherId == id)
            .OrderByDescending(subscription => subscription.Id)
            .Select(SubscriptionMappings.ToDto)
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow().UtcDateTime;

        return Result<TeacherDetailsDto>.Ok(new TeacherDetailsDto(
            teacher.Id,
            teacher.FullName,
            teacher.Subject,
            teacher.Phone,
            teacher.Email,
            teacher.Notes,
            teacher.PlanId,
            teacher.Plan?.Name,
            teacher.Status,
            SubscriptionRules.StateOf(teacher.Status, teacher.SubscriptionEndsAt, now, settings),
            teacher.SubscriptionEndsAt,
            SubscriptionRules.DaysLeft(teacher.SubscriptionEndsAt, now),
            teacher.SuspensionReason,
            teacher.CreatedAt,
            subscriptions));
    }

    public async Task<Result<TeacherDetailsDto>> UpdateAsync(int id, UpdateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        var teacher = await unitOfWork.Repository<Teacher>().GetByIdAsync(id, cancellationToken);

        if (teacher is null)
        {
            return Result<TeacherDetailsDto>.NotFound("المدرس مش موجود.");
        }

        teacher.FullName = request.FullName.Trim();

        teacher.Subject = request.Subject.Trim();

        teacher.Phone = request.Phone.Trim();

        teacher.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        teacher.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> SuspendAsync(int id, SuspendTeacherRequest request, CancellationToken cancellationToken = default)
    {
        var teacher = await unitOfWork.Repository<Teacher>().GetByIdAsync(id, cancellationToken);

        if (teacher is null)
        {
            return Result.NotFound("المدرس مش موجود.");
        }

        if (teacher.Status == TeacherStatus.Suspended)
        {
            return Result.Conflict("الحساب موقوف بالفعل.");
        }

        teacher.Status = TeacherStatus.Suspended;

        teacher.SuspensionReason = request.Reason.Trim();

        await notifications.QueueAsync(
            await recipients.TeacherOwnersAsync(teacher.Id, cancellationToken),
            teacher.Id,
            NotificationTypes.AccountSuspended,
            "تم إيقاف الحساب",
            $"الحساب اتوقف. السبب: {teacher.SuspensionReason}",
            null,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        teacherAccess.Forget(teacher.Id);

        return Result.Ok("الحساب اتوقف.");
    }

    public async Task<Result> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var teacher = await unitOfWork.Repository<Teacher>().GetByIdAsync(id, cancellationToken);

        if (teacher is null)
        {
            return Result.NotFound("المدرس مش موجود.");
        }

        if (teacher.Status != TeacherStatus.Suspended)
        {
            return Result.Conflict("الحساب مش موقوف.");
        }

        var hasPaid = await unitOfWork.Repository<TeacherSubscription>().AnyAsync(subscription => subscription.TeacherId == id && !subscription.IsCancelled, cancellationToken);

        teacher.Status = hasPaid ? TeacherStatus.Active : TeacherStatus.Trial;

        teacher.SuspensionReason = null;

        await notifications.QueueAsync(
            await recipients.TeacherOwnersAsync(teacher.Id, cancellationToken),
            teacher.Id,
            NotificationTypes.AccountActivated,
            "تم تفعيل الحساب",
            "الحساب رجع شغال تاني.",
            null,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        teacherAccess.Forget(teacher.Id);

        return Result.Ok("الحساب اتفعّل.");
    }
}
