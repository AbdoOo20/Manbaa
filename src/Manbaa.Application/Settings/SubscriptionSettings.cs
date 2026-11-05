namespace Manbaa.Application.Settings;

public class SubscriptionSettings
{
    public const string SectionName = "Subscriptions";

    /// <summary>Length of the free trial given to a new teacher.</summary>
    public int TrialDays { get; set; } = 14;

    /// <summary>Days after the end date the teacher can still use the system.</summary>
    public int GraceDays { get; set; } = 3;

    /// <summary>How many days before the end date the teacher and the admin are warned.</summary>
    public int ExpiryWarningDays { get; set; } = 7;
}
