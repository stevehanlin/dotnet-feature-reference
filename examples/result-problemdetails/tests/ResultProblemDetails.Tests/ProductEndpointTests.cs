using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ResultProblemDetails.Api;

namespace ResultProblemDetails.Tests;

public class ProductEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_returns_201_with_location()
    {
        var response = await _client.PostAsJsonAsync("/products", new CreateProductRequest("Widget-A", 9.99m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.Equal($"/products/{product!.Id}", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Get_unknown_product_returns_404_problem_details()
    {
        var response = await _client.GetAsync($"/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Product.NotFound", body.GetProperty("title").GetString());
        Assert.Equal(404, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Create_with_invalid_input_returns_400_validation_problem()
    {
        var response = await _client.PostAsJsonAsync("/products", new CreateProductRequest("", -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Price", out _));
    }

    [Fact]
    public async Task Create_duplicate_returns_409()
    {
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("Widget-Dup", 5m));
        var response = await _client.PostAsJsonAsync("/products", new CreateProductRequest("widget-dup", 5m));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
