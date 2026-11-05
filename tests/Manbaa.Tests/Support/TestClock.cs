namespace Manbaa.Tests.Support;

public sealed class TestClock : TimeProvider
{
    public DateTime Now { get; set; } = new(2026, 11, 5, 10, 0, 0, DateTimeKind.Utc);

    public override DateTimeOffset GetUtcNow() => new(Now);
}
