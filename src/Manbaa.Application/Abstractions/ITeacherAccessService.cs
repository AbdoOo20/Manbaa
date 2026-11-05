namespace Manbaa.Application.Abstractions;

/// <summary>
/// Tells whether a teacher's account may be used right now: not suspended, and the subscription
/// (or trial) has not ended beyond the grace period. Cached for a few seconds; <see cref="Forget" /> clears it.
/// </summary>
public interface ITeacherAccessService
{
    const string NotActiveMessage = "اشتراك الحساب ده منتهي أو موقوف. تواصل مع إدارة منبع.";

    Task<bool> IsActiveAsync(int teacherId, CancellationToken cancellationToken = default);

    void Forget(int teacherId);
}
