using System.Net;
using System.Net.Http.Json;
using HttpClients.Api;

namespace HttpClients.Tests;

public class HttpClientApproachTests : HttpClientTestBase
{
    public const string ApiKeyHeader = "X-Api-Key";

    [Theory]
    [InlineData("basic")]
    [InlineData("named")]
    [InlineData("typed")]
    [InlineData("typed-interface")]
    [InlineData("keyed")]
    public async Task GivenAnyApproach_WhenFetchingAQuote_ThenReturnsQuoteAndRequestsTheSameUpstreamUrl(string approach)
    {
        var response = await GetAsync($"/quotes/{approach}/7");

        var result = await response.Content.ReadFromJsonAsync<QuoteResult>();
        Assert.Equal(new QuoteResult(approach, new Quote(7, "quote 7")), result);
        Assert.Equal("https://quotes.example.test/quotes/7", Upstream.Single.RequestUri!.ToString());
    }

    [Theory]
    [InlineData("basic")]
    [InlineData("named")]
    [InlineData("typed")]
    [InlineData("typed-interface")]
    [InlineData("keyed")]
    public async Task GivenAnyApproach_WhenSendingRequest_ThenClientDefaultsApply(string approach)
    {
        await GetAsync($"/quotes/{approach}/7");

        Assert.Contains("dotnet-feature-reference", Upstream.Single.Headers.UserAgent.ToString());
    }

    [Theory]
    [InlineData("named")]
    [InlineData("typed")]
    [InlineData("typed-interface")]
    [InlineData("keyed")]
    public async Task GivenClientWithApiKeyHandler_WhenSendingRequest_ThenHandlerAddsApiKey(string approach)
    {
        await GetAsync($"/quotes/{approach}/7");

        Assert.Equal("demo-key", Assert.Single(Upstream.Single.Headers.GetValues(ApiKeyHeader)));
    }

    [Fact]
    public async Task GivenUnnamedClient_WhenSendingRequest_ThenApiKeyHandlerIsNotApplied()
    {
        await GetAsync("/quotes/basic/7");

        Assert.False(Upstream.Single.Headers.Contains(ApiKeyHeader));
    }

    [Theory]
    [InlineData("basic")]
    [InlineData("named")]
    [InlineData("typed")]
    [InlineData("typed-interface")]
    [InlineData("keyed")]
    public async Task GivenUpstreamReturnsNotFound_WhenFetching_ThenRespondsBadGateway(string approach)
    {
        var response = await GetAsync($"/quotes/{approach}/404");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task GivenUpstreamReturnsNullBody_WhenFetching_ThenRespondsBadGateway()
    {
        var response = await GetAsync("/quotes/typed/0");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }
}

public class HttpClientConfigurationTests() : HttpClientTestBase(
    ("Quotes:BaseUrl", "https://other.example.test"),
    ("Quotes:ApiKey", "rotated-key"))
{
    [Fact]
    public async Task GivenQuotesSectionOverridden_WhenUsingNamedClient_ThenBaseAddressAndApiKeyFollowConfiguration()
    {
        await GetAsync("/quotes/named/3");

        Assert.Equal("https://other.example.test/quotes/3", Upstream.Single.RequestUri!.ToString());
        Assert.Equal("rotated-key", Assert.Single(Upstream.Single.Headers.GetValues(HttpClientApproachTests.ApiKeyHeader)));
    }
}
