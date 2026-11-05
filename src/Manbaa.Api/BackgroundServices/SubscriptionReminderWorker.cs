using Manbaa.Application.Services.Subscriptions;

namespace Manbaa.Api.BackgroundServices;

/// <summary>Every few hours, warns teachers whose subscription or trial ends soon, and tells the system admins.</summary>
public class SubscriptionReminderWorker(IServiceScopeFactory scopeFactory, ILogger<SubscriptionReminderWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var reminders = scope.ServiceProvider.GetRequiredService<ISubscriptionReminderService>();

                var sent = await reminders.SendExpiryRemindersAsync(stoppingToken);

                if (sent > 0)
                {
                    logger.LogInformation("Sent expiry reminders for {Count} teachers.", sent);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Expiry reminders failed; will try again next round.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
