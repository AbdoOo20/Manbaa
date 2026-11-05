namespace Manbaa.Application.Services.Notifications;

/// <summary>Finds who should receive a notification, so services never hard-code user ids.</summary>
public interface INotificationRecipients
{
    Task<IReadOnlyList<int>> SystemAdminsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> TeacherOwnersAsync(int teacherId, CancellationToken cancellationToken = default);
}
