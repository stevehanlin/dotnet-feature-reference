using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ExtensionMethodsDi.Api;

namespace ExtensionMethodsDi.Tests;

public class ExtensionTests : AppTestBase
{
    [Fact]
    public async Task GivenFakeClockRegisteredLast_WhenGreeting_ThenGreeterUsesConfiguredGreetingAndFakeTime()
    {
        var client = CreateClient(WithClockAt("2026-01-01T09:30:00Z"));

        Assert.Equal("Hello, Ada! (09:30 UTC)", await client.GetStringAsync("/greet/Ada"));
    }

    [Fact]
    public async Task GivenAuditPrefixInConfiguration_WhenFormattingAuditMessage_ThenUsesConfiguredPrefix()
    {
        var client = CreateClient(WithSetting("AuditLog:Prefix", "SEC"));

        Assert.Equal("[SEC] login", await client.GetStringAsync("/audit/login"));
    }

    [Fact]
    public async Task GivenAnyRequest_WhenPassingThroughPipeline_ThenMiddlewareExtensionStampsResponse()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/greet/Ada");

        Assert.Equal("true", response.Headers.GetValues("X-Stamped").Single());
    }

    [Fact]
    public void GivenEmptyAndConfiguredCollections_WhenCheckingHasGreeter_ThenExtensionPropertyReflectsRegistration()
    {
        var empty = new ServiceCollection();
        var configured = new ServiceCollection().AddGreeter();

        Assert.False(empty.HasGreeter);
        Assert.True(configured.HasGreeter);
    }

    [Fact]
    public void GivenWebApplicationBuilderType_WhenCallingStaticExtensionMember_ThenRegistersTimeProvider()
    {
        var builder = WebApplicationBuilder.CreateWithDefaults([]);

        Assert.Contains(builder.Services, descriptor => descriptor.ServiceType == typeof(TimeProvider));
    }
}
