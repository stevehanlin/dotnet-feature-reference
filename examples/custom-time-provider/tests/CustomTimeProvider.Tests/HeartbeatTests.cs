using CustomTimeProvider.Api;

namespace CustomTimeProvider.Tests;

public class HeartbeatTests : ClockTestBase
{
    [Fact]
    public void GivenThirtySecondInterval_WhenFakeClockAdvancesNinetySeconds_ThenTimerFiresThreeTimesThroughTheWrapper()
    {
        using var heartbeat = new Heartbeat(ClockAt("2026-01-05T14:30:00Z"), TimeSpan.FromSeconds(30));
        Assert.Equal(0, heartbeat.Beats);

        Fake.Advance(TimeSpan.FromSeconds(90));

        Assert.Equal(3, heartbeat.Beats);
    }
}
