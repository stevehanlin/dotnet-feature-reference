using CustomTimeProvider.Api;

namespace CustomTimeProvider.Tests;

public class StoreHoursTests : ClockTestBase
{
    [Theory]
    [InlineData("2026-01-05T14:30:00Z", true)]    // Mon 09:30 local
    [InlineData("2026-01-05T13:59:00Z", false)]   // Mon 08:59 local, before opening
    [InlineData("2026-01-05T22:00:00Z", false)]   // Mon 17:00 local, closing time is exclusive
    [InlineData("2026-01-10T15:00:00Z", false)]   // Saturday
    public void GivenUtcInstant_WhenCheckingIsOpen_ThenEvaluatesInBusinessTime(string utc, bool expected)
    {
        var hours = new StoreHours(ClockAt(utc));

        Assert.Equal(expected, hours.IsOpen());
    }
}
