using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExceptionHandling.Tests;

public class ExceptionHandlingTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Domain_exception_becomes_its_own_status_and_title()
    {
        var response = await _client.GetAsync("/orders/42");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Order not found", body.GetProperty("title").GetString());
        Assert.Equal("Order 42 does not exist.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Different_domain_exception_uses_same_handler_with_other_status()
    {
        var response = await _client.GetAsync("/quantity/-5");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unexpected_exception_falls_through_to_global_handler_without_leaking_details()
    {
        var response = await _client.GetAsync("/boom");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret internal detail", text);
        Assert.Contains("An unexpected error occurred", text);
    }

    [Fact]
    public async Task Problem_details_include_customized_trace_id()
    {
        var response = await _client.GetAsync("/orders/42");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task Unknown_route_gets_problem_details_from_status_code_pages()
    {
        var response = await _client.GetAsync("/nope");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Success_path_is_unaffected()
    {
        var response = await _client.GetAsync("/orders/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
