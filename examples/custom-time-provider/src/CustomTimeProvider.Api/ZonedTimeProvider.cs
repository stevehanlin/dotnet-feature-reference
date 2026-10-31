namespace CustomTimeProvider.Api;

/// <summary>
/// Reports a fixed business time zone as "local" time, whatever zone the server runs in.
/// Everything else (UTC now, timestamps, timers) is delegated to <paramref name="inner"/>,
/// so it composes with the system clock in production and FakeTimeProvider in tests.
/// </summary>
public sealed class ZonedTimeProvider(TimeProvider inner, TimeZoneInfo zone)
    : TimeProvider
{
    // GetLocalNow() is not virtual: it is built from GetUtcNow() + LocalTimeZone, so overriding the zone is enough.
    public override TimeZoneInfo LocalTimeZone => zone;

    public override DateTimeOffset GetUtcNow() => inner.GetUtcNow();

    // Elapsed-time measurement must use the same source as the inner clock.
    public override long GetTimestamp() => inner.GetTimestamp();

    public override long TimestampFrequency => inner.TimestampFrequency;

    // Timers must come from the inner clock too, or FakeTimeProvider.Advance() would never fire them.
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
        inner.CreateTimer(callback, state, dueTime, period);
}
