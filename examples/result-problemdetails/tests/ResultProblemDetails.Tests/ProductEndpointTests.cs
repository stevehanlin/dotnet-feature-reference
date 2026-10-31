using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ResultProblemDetails.Api;

namespace ResultProblemDetails.Tests;

public class ProductEndpointTests(WebApplicationFactory<Program> factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task GivenValidRequest_WhenCreatingProduct_ThenReturnsCreatedWithLocation()
    {
        var response = await PostProductAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.Equal($"/products/{product!.Id}", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task GivenUnknownId_WhenGettingProduct_ThenReturnsNotFoundProblemDetails()
    {
        var response = await Client.GetAsync($"/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var body = await ReadJsonAsync(response);
        Assert.Equal("Product.NotFound", body.GetProperty("title").GetString());
        Assert.Equal(404, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task GivenExistingProduct_WhenGettingProduct_ThenReturnsOkWithProduct()
    {
        var created = await CreateProductAsync();

        var response = await Client.GetAsync($"/products/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created, await response.Content.ReadFromJsonAsync<Product>());
    }

    [Fact]
    public async Task GivenInvalidRequest_WhenCreatingProduct_ThenReturnsValidationProblemPerField()
    {
        var response = await Client.PostAsJsonAsync("/products", new CreateProductRequest("", -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await ReadJsonAsync(response)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Price", out _));
    }

    [Fact]
    public async Task GivenDuplicateName_WhenCreatingProduct_ThenReturnsConflict()
    {
        var existing = await CreateProductAsync();

        var response = await PostProductAsync(existing.Name.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
