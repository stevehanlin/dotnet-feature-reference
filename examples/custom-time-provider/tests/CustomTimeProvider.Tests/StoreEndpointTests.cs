using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using CustomTimeProvider.Api;

namespace CustomTimeProvider.Tests;

public class StoreEndpointTests : ClockTestBase, IDisposable
{
    private WebApplicationFactory<Program>? _factory;

    [Fact]
    public async Task GivenClosingTimeClock_WhenGettingStoreStatus_ThenReportsClosedInBusinessZone()
    {
        var clock = ClockAt("2026-01-05T22:00:00Z");   // Mon 17:00 local
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<TimeProvider>(clock)));
        using var client = _factory.CreateClient();

        var json = JsonDocument.Parse(await client.GetStringAsync("/store/status")).RootElement;

        Assert.False(json.GetProperty("isOpen").GetBoolean());
        Assert.Equal("America/New_York", json.GetProperty("zone").GetString());
    }

    public void Dispose() => _factory?.Dispose();
}
