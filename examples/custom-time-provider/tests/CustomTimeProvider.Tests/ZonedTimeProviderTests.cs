namespace CustomTimeProvider.Tests;

public class ZonedTimeProviderTests : ClockTestBase
{
    [Fact]
    public void GivenWinterUtcInstant_WhenGettingLocalNow_ThenUsesConfiguredZoneNotServerZone()
    {
        var clock = ClockAt("2026-01-05T14:30:00Z");   // January: New York is UTC-5

        Assert.Equal(new TimeSpan(9, 30, 0), clock.GetLocalNow().TimeOfDay);
        Assert.Equal(TimeSpan.FromHours(-5), clock.GetLocalNow().Offset);
    }

    [Fact]
    public void GivenSummerUtcInstant_WhenGettingLocalNow_ThenAppliesDaylightSavingOffset()
    {
        var clock = ClockAt("2026-07-06T13:30:00Z");   // July: New York is UTC-4

        Assert.Equal(new TimeSpan(9, 30, 0), clock.GetLocalNow().TimeOfDay);
        Assert.Equal(TimeSpan.FromHours(-4), clock.GetLocalNow().Offset);
    }

    [Fact]
    public void GivenInnerClock_WhenGettingUtcNow_ThenDelegatesUnchanged()
    {
        var clock = ClockAt("2026-01-05T14:30:00Z");

        Assert.Equal(Fake.GetUtcNow(), clock.GetUtcNow());
    }

    [Fact]
    public void GivenTimestamp_WhenInnerClockAdvances_ThenElapsedTimeFollowsInnerClock()
    {
        var clock = ClockAt("2026-01-05T14:30:00Z");
        var start = clock.GetTimestamp();

        Fake.Advance(TimeSpan.FromMinutes(5));

        Assert.Equal(TimeSpan.FromMinutes(5), clock.GetElapsedTime(start));
    }
}
