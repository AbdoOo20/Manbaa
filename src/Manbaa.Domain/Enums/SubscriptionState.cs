namespace Manbaa.Domain.Enums;

/// <summary>Where a teacher's subscription stands right now. Calculated from the status and the end date, never stored.</summary>
public enum SubscriptionState
{
    Trial = 1,

    Active = 2,

    ExpiringSoon = 3,

    InGrace = 4,

    Expired = 5,

    Suspended = 6
}
