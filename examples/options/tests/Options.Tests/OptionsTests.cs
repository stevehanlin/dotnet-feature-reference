using Microsoft.Extensions.Options;

namespace Options.Tests;

public class OptionsTests : OptionsTestBase
{
    [Fact]
    public async Task GivenGreetingConfiguration_WhenReadingIOptions_ThenBindsValues()
    {
        var client = CreateClient(("Greeting:Message", "Hi"), ("Greeting:Repeat", "3"));

        Assert.Equal("Hi Hi Hi", await client.GetStringAsync("/greeting/ioptions"));
    }

    [Fact]
    public async Task GivenGreetingConfiguration_WhenReadingSnapshotAndMonitor_ThenBothSeeTheSameBoundValues()
    {
        var client = CreateClient(("Greeting:Message", "Yo"), ("Greeting:Repeat", "1"));

        Assert.Equal("Yo", await client.GetStringAsync("/greeting/snapshot"));
        Assert.Equal("Yo", await client.GetStringAsync("/greeting/monitor"));
    }

    [Fact]
    public void GivenRepeatOutOfRange_WhenStartingHost_ThenDataAnnotationValidationFailsAtStartup()
    {
        var ex = Assert.Throws<OptionsValidationException>(() => CreateClient(("Greeting:Repeat", "99")));

        Assert.Contains(ex.Failures, failure => failure.Contains("Repeat"));
    }

    [Fact]
    public void GivenBetaEnabledWithoutKey_WhenStartingHost_ThenCustomValidatorFailsAtStartup()
    {
        var ex = Assert.Throws<OptionsValidationException>(() => CreateClient(("Features:EnableBeta", "true")));

        Assert.Contains(ex.Failures, failure => failure.Contains("BetaKey"));
    }

    [Fact]
    public async Task GivenBetaEnabledWithKey_WhenReadingFeatures_ThenCustomValidatorPassesAndKeyIsReturned()
    {
        var client = CreateClient(("Features:EnableBeta", "true"), ("Features:BetaKey", "abc"));

        Assert.Contains("\"betaKey\":\"abc\"", await client.GetStringAsync("/features"));
    }
}
