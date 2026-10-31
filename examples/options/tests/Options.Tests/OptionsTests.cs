using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Options.Tests;

public class OptionsTests
{
    private static WebApplicationFactory<Program> Factory(params (string Key, string? Value)[] overrides) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(overrides.ToDictionary(o => o.Key, o => o.Value))));

    [Fact]
    public async Task Binds_values_from_configuration()
    {
        await using var factory = Factory(("Greeting:Message", "Hi"), ("Greeting:Repeat", "3"));
        using var client = factory.CreateClient();

        Assert.Equal("Hi Hi Hi", await client.GetStringAsync("/greeting/ioptions"));
    }

    [Fact]
    public async Task Snapshot_and_monitor_read_the_same_bound_values()
    {
        await using var factory = Factory(("Greeting:Message", "Yo"), ("Greeting:Repeat", "1"));
        using var client = factory.CreateClient();

        Assert.Equal("Yo", await client.GetStringAsync("/greeting/snapshot"));
        Assert.Equal("Yo", await client.GetStringAsync("/greeting/monitor"));
    }

    [Fact]
    public void Invalid_data_annotations_fail_at_startup()
    {
        using var factory = Factory(("Greeting:Repeat", "99"));

        var ex = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
        Assert.Contains(ex.Failures, f => f.Contains("Repeat"));
    }

    [Fact]
    public void Custom_validator_enforces_cross_property_rule()
    {
        using var factory = Factory(("Features:EnableBeta", "true"));

        var ex = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
        Assert.Contains(ex.Failures, f => f.Contains("BetaKey"));
    }

    [Fact]
    public async Task Custom_validator_passes_when_key_supplied()
    {
        await using var factory = Factory(("Features:EnableBeta", "true"), ("Features:BetaKey", "abc"));
        using var client = factory.CreateClient();

        var json = await client.GetStringAsync("/features");
        Assert.Contains("\"betaKey\":\"abc\"", json);
    }
}
