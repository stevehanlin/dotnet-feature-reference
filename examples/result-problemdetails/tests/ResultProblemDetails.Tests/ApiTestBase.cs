using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ResultProblemDetails.Api;

namespace ResultProblemDetails.Tests;

/// <summary>
/// Shared setup/teardown for endpoint tests: one factory per test class, one client per test,
/// and helpers so each test only states what is specific to it.
/// </summary>
public abstract class ApiTestBase(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected HttpClient Client { get; } = factory.CreateClient();

    // The product store is shared within a class, so every test uses its own product name.
    protected static string UniqueName() => $"Product-{Guid.NewGuid():N}";

    protected Task<HttpResponseMessage> PostProductAsync(string? name = null, decimal price = 9.99m) =>
        Client.PostAsJsonAsync("/products", new CreateProductRequest(name ?? UniqueName(), price));

    protected async Task<Product> CreateProductAsync(string? name = null)
    {
        var response = await PostProductAsync(name);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Product>())!;
    }

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    public void Dispose() => Client.Dispose();
}
