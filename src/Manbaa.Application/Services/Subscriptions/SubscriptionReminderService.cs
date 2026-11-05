using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Services.Notifications;

using Manbaa.Application.Settings;

using Manbaa.Domain.Entities;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.Services.Subscriptions;

public class SubscriptionReminderService(
    IUnitOfWork unitOfWork,
    INotificationService notifications,
    INotificationRecipients recipients,
    IOptions<SubscriptionSettings> settingsOptions,
    TimeProvider clock) : ISubscriptionReminderService
{
    private readonly SubscriptionSettings settings = settingsOptions.Value;

    public async Task<int> SendExpiryRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var warningEdge = now.AddDays(settings.ExpiryWarningDays);

        var teachers = await unitOfWork.Repository<Teacher>().Query(asNoTracking: false)
            .Where(teacher => teacher.Status != TeacherStatus.Suspended
                && teacher.SubscriptionEndsAt >= now
                && teacher.SubscriptionEndsAt <= warningEdge
                && teacher.ExpiryReminderSentAt == null)
            .ToListAsync(cancellationToken);

        if (teachers.Count == 0)
        {
            return 0;
        }

        var admins = await recipients.SystemAdminsAsync(cancellationToken);

        foreach (var teacher in teachers)
        {
            var daysLeft = Math.Max(SubscriptionRules.DaysLeft(teacher.SubscriptionEndsAt, now) ?? 0, 0);

            var what = teacher.Status == TeacherStatus.Trial ? "الفترة التجريبية" : "اشتراكك";

            await notifications.QueueAsync(
                await recipients.TeacherOwnersAsync(teacher.Id, cancellationToken),
                teacher.Id,
                NotificationTypes.SubscriptionExpiringSoon,
                "الاشتراك قرّب يخلص",
                $"{what} هيخلص بعد {daysLeft} يوم ({teacher.SubscriptionEndsAt:yyyy/MM/dd}). جدّد عشان الطلاب ما يتوقفوش.",
                "/subscription",
                cancellationToken);

            await notifications.QueueAsync(
                admins,
                teacher.Id,
                NotificationTypes.SubscriptionExpiringSoon,
                "اشتراك مدرس قرّب يخلص",
                $"اشتراك {teacher.FullName} هيخلص بعد {daysLeft} يوم.",
                $"/admin/teachers/{teacher.Id}",
                cancellationToken);

            teacher.ExpiryReminderSentAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return teachers.Count;
    }
}
