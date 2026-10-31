using Microsoft.Extensions.Time.Testing;
using CustomTimeProvider.Api;

namespace CustomTimeProvider.Tests;

/// <summary>
/// Shared setup: a fake inner clock and the business zone. xUnit creates a new instance per test,
/// so every test starts with its own clock and nothing needs resetting.
/// </summary>
public abstract class ClockTestBase
{
    protected static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    protected FakeTimeProvider Fake { get; private set; } = new();

    /// <summary>Starts the fake clock at <paramref name="utc"/> and returns the zoned wrapper around it.</summary>
    protected ZonedTimeProvider ClockAt(string utc)
    {
        Fake = new FakeTimeProvider(DateTimeOffset.Parse(utc));
        return new ZonedTimeProvider(Fake, NewYork);
    }
}
