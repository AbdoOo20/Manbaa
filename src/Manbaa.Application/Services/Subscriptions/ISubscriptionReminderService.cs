namespace Manbaa.Application.Services.Subscriptions;

public interface ISubscriptionReminderService
{
    /// <summary>Warns each teacher whose trial or subscription ends soon (once per period) and tells the system admins. Returns how many were warned.</summary>
    Task<int> SendExpiryRemindersAsync(CancellationToken cancellationToken = default);
}
