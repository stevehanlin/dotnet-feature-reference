using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using ExtensionMethodsDi.Api;

namespace ExtensionMethodsDi.Tests;

public class ExtensionTests
{
    [Fact]
    public async Task Greeter_uses_configured_greeting_and_replaceable_clock()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.ConfigureServices(s => s.AddSingleton<TimeProvider>(clock)));   // registered last, so it wins

        using var client = factory.CreateClient();
        var text = await client.GetStringAsync("/greet/Ada");

        Assert.Equal("Hello, Ada! (09:30 UTC)", text);
    }

    [Fact]
    public async Task Audit_prefix_comes_from_configuration()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AuditLog:Prefix"] = "SEC",
            })));

        using var client = factory.CreateClient();

        Assert.Equal("[SEC] login", await client.GetStringAsync("/audit/login"));
    }

    [Fact]
    public async Task Middleware_extension_stamps_responses()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/greet/Ada");

        Assert.Equal("true", response.Headers.GetValues("X-Stamped").Single());
    }

    [Fact]
    public void Extension_property_reports_registration()
    {
        var empty = new ServiceCollection();
        var configured = new ServiceCollection().AddGreeter();

        Assert.False(empty.HasGreeter);
        Assert.True(configured.HasGreeter);
    }

    [Fact]
    public void Static_extension_member_is_callable_on_the_type()
    {
        var builder = WebApplicationBuilder.CreateWithDefaults([]);

        Assert.Contains(builder.Services, d => d.ServiceType == typeof(TimeProvider));
    }
}
