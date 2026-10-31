using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExceptionHandling.Tests;

public class ExceptionHandlingTests(WebApplicationFactory<Program> factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task GivenMissingOrder_WhenGettingOrder_ThenReturnsNotFoundWithDomainTitle()
    {
        var (response, body) = await GetProblemAsync("/orders/42");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Order not found", body.GetProperty("title").GetString());
        Assert.Equal("Order 42 does not exist.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task GivenExistingOrder_WhenGettingOrder_ThenReturnsOk()
    {
        var response = await Client.GetAsync("/orders/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GivenNegativeQuantity_WhenGettingQuantity_ThenSameHandlerReturnsBadRequest()
    {
        var response = await Client.GetAsync("/quantity/-5");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenValidQuantity_WhenGettingQuantity_ThenReturnsOkWithoutTouchingHandlers()
    {
        var response = await Client.GetAsync("/quantity/5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("5", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GivenUnexpectedException_WhenCallingBoom_ThenGlobalHandlerReturns500WithoutLeakingDetails()
    {
        var response = await Client.GetAsync("/boom");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret internal detail", text);
        Assert.Contains("An unexpected error occurred", text);
    }

    [Fact]
    public async Task GivenHandledError_WhenReadingProblemDetails_ThenIncludesCustomizedTraceId()
    {
        var (_, body) = await GetProblemAsync("/orders/42");

        Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task GivenActivityTracingEnabled_WhenHandlingError_ThenTraceIdIsTheW3cActivityId()
    {
        using var tracing = new ActivityTracingScope();

        var (_, body) = await GetProblemAsync("/orders/42");

        Assert.StartsWith("00-", body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task GivenUnknownRoute_WhenRequesting_ThenStatusCodePagesReturnProblemDetails()
    {
        var response = await Client.GetAsync("/nope");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }
}
