using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExceptionHandling.Tests;

/// <summary>Shared setup/teardown: one factory per class, one client per test, request and parsing helpers.</summary>
public abstract class ApiTestBase(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected HttpClient Client { get; } = factory.CreateClient();

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    protected async Task<(HttpResponseMessage Response, JsonElement Body)> GetProblemAsync(string path)
    {
        var response = await Client.GetAsync(path);
        return (response, await ReadJsonAsync(response));
    }

    public void Dispose() => Client.Dispose();

    /// <summary>Turns on ASP.NET Core activities for the lifetime of the scope, then switches them off again.</summary>
    protected sealed class ActivityTracingScope : IDisposable
    {
        private readonly ActivityListener _listener = new()
        {
            ShouldListenTo = source => source.Name == "Microsoft.AspNetCore",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };

        public ActivityTracingScope() => ActivitySource.AddActivityListener(_listener);

        public void Dispose() => _listener.Dispose();
    }
}
