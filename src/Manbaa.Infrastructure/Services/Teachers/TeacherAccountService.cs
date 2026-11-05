using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Teachers;

using Manbaa.Application.Services.Notifications;

using Manbaa.Application.Services.Teachers;

using Manbaa.Application.Settings;

using Manbaa.Domain.Constants;

using Manbaa.Domain.Entities;

using Manbaa.Domain.Enums;

namespace Manbaa.Infrastructure.Services.Teachers;

public class TeacherAccountService(
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    ITeacherService teachers,
    INotificationService notifications,
    INotificationRecipients recipients,
    IOptions<SubscriptionSettings> settingsOptions,
    TimeProvider clock) : ITeacherAccountService
{
    public async Task<Result<TeacherDetailsDto>> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        var phone = request.Phone.Trim();

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        if (await unitOfWork.Repository<Teacher>().AnyAsync(teacher => teacher.Phone == phone, cancellationToken))
        {
            return Result<TeacherDetailsDto>.Conflict("فيه مدرس مسجّل بنفس رقم الموبايل.");
        }

        if (email is not null && await userManager.FindByEmailAsync(email) is not null)
        {
            return Result<TeacherDetailsDto>.Conflict("الإيميل ده مستخدم في حساب تاني.");
        }

        if (await userManager.Users.AnyAsync(user => user.TeacherId != null && user.PhoneNumber == phone, cancellationToken) && email is null)
        {
            return Result<TeacherDetailsDto>.Conflict("رقم الموبايل ده مستخدم في حساب تاني. اكتب إيميل للدخول.");
        }

        if (request.PlanId is { } planId && !await unitOfWork.Repository<Plan>().AnyAsync(plan => plan.Id == planId && plan.IsActive, cancellationToken))
        {
            return Result<TeacherDetailsDto>.BadRequest("الباقة مش موجودة أو موقوفة.");
        }

        var trialEndsAt = clock.GetUtcNow().UtcDateTime.AddDays(settingsOptions.Value.TrialDays);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var teacher = new Teacher
            {
                FullName = request.FullName.Trim(),

                Subject = request.Subject.Trim(),

                Phone = phone,

                Email = email,

                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),

                PlanId = request.PlanId,

                Status = TeacherStatus.Trial,

                TrialEndsAt = trialEndsAt,

                SubscriptionEndsAt = trialEndsAt
            };

            await unitOfWork.Repository<Teacher>().AddAsync(teacher, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var user = new User
            {
                UserName = $"teacher-{teacher.Id}",

                Email = email,

                EmailConfirmed = email is not null,

                PhoneNumber = phone,

                FullName = teacher.FullName,

                TeacherId = teacher.Id
            };

            var created = await userManager.CreateAsync(user, request.Password);

            if (!created.Succeeded)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);

                return Result<TeacherDetailsDto>.BadRequest(created.Errors.Select(error => error.Description));
            }

            await userManager.AddToRoleAsync(user, RoleNames.Teacher);

            await notifications.QueueAsync(
                await recipients.SystemAdminsAsync(cancellationToken),
                teacher.Id,
                NotificationTypes.TeacherJoined,
                "مدرس جديد",
                $"{teacher.FullName} ({teacher.Subject}) اتضاف وبدأ الفترة التجريبية.",
                $"/admin/teachers/{teacher.Id}",
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var details = await teachers.GetByIdAsync(teacher.Id, cancellationToken);

            return Result<TeacherDetailsDto>.Created(details.Data!, "المدرس اتضاف.");
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            throw;
        }
    }
}
