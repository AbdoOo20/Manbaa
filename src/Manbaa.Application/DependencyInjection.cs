using Microsoft.Extensions.DependencyInjection;

using Manbaa.Application.Services.Audit;

using Manbaa.Application.Services.Dashboard;

using Manbaa.Application.Services.Notifications;

using Manbaa.Application.Services.Plans;

using Manbaa.Application.Services.Subscriptions;

using Manbaa.Application.Services.Teachers;

namespace Manbaa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<INotificationRecipients, NotificationRecipients>();

        services.AddScoped<INotificationService, NotificationService>();

        services.AddScoped<IPlanService, PlanService>();

        services.AddScoped<ITeacherService, TeacherService>();

        services.AddScoped<ISubscriptionService, SubscriptionService>();

        services.AddScoped<ISubscriptionReminderService, SubscriptionReminderService>();

        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
