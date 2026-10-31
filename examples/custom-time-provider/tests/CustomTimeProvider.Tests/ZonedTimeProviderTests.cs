using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using CustomTimeProvider.Api;
using System.Text.Json;

namespace CustomTimeProvider.Tests;

public class ZonedTimeProviderTests
{
    private static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    private static ZonedTimeProvider ZonedAt(string utc, out FakeTimeProvider fake)
    {
        fake = new FakeTimeProvider(DateTimeOffset.Parse(utc));
        return new ZonedTimeProvider(fake, NewYork);
    }

    [Fact]
    public void Local_time_uses_the_configured_zone_not_the_server_zone()
    {
        var clock = ZonedAt("2026-01-05T14:30:00Z", out _);   // January: New York is UTC-5

        Assert.Equal(new TimeSpan(9, 30, 0), clock.GetLocalNow().TimeOfDay);
        Assert.Equal(TimeSpan.FromHours(-5), clock.GetLocalNow().Offset);
    }

    [Fact]
    public void Daylight_saving_is_applied_by_the_zone()
    {
        var clock = ZonedAt("2026-07-06T13:30:00Z", out _);   // July: New York is UTC-4

        Assert.Equal(new TimeSpan(9, 30, 0), clock.GetLocalNow().TimeOfDay);
        Assert.Equal(TimeSpan.FromHours(-4), clock.GetLocalNow().Offset);
    }

    [Fact]
    public void UtcNow_is_delegated_unchanged()
    {
        var clock = ZonedAt("2026-01-05T14:30:00Z", out var fake);

        Assert.Equal(fake.GetUtcNow(), clock.GetUtcNow());
    }

    [Fact]
    public void Elapsed_time_follows_the_inner_clock()
    {
        var clock = ZonedAt("2026-01-05T14:30:00Z", out var fake);
        var start = clock.GetTimestamp();

        fake.Advance(TimeSpan.FromMinutes(5));

        Assert.Equal(TimeSpan.FromMinutes(5), clock.GetElapsedTime(start));
    }

    [Theory]
    [InlineData("2026-01-05T14:30:00Z", true)]    // Mon 09:30 local
    [InlineData("2026-01-05T13:59:00Z", false)]   // Mon 08:59 local, before opening
    [InlineData("2026-01-05T22:00:00Z", false)]   // Mon 17:00 local, closing time is exclusive
    [InlineData("2026-01-10T15:00:00Z", false)]   // Saturday
    public void StoreHours_is_evaluated_in_business_time(string utc, bool expected)
    {
        var hours = new StoreHours(ZonedAt(utc, out _));

        Assert.Equal(expected, hours.IsOpen());
    }

    [Fact]
    public void Timers_fire_when_the_fake_clock_advances_through_the_wrapper()
    {
        var clock = ZonedAt("2026-01-05T14:30:00Z", out var fake);
        using var heartbeat = new Heartbeat(clock, TimeSpan.FromSeconds(30));

        Assert.Equal(0, heartbeat.Beats);

        fake.Advance(TimeSpan.FromSeconds(90));

        Assert.Equal(3, heartbeat.Beats);
    }

    [Fact]
    public async Task Endpoint_reports_status_for_the_injected_clock()
    {
        var clock = ZonedAt("2026-01-05T22:00:00Z", out _);   // Mon 17:00 local
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.ConfigureServices(s => s.AddSingleton<TimeProvider>(clock)));
        using var client = factory.CreateClient();

        var json = JsonDocument.Parse(await client.GetStringAsync("/store/status")).RootElement;

        Assert.False(json.GetProperty("isOpen").GetBoolean());
        Assert.Equal("America/New_York", json.GetProperty("zone").GetString());
    }
}
