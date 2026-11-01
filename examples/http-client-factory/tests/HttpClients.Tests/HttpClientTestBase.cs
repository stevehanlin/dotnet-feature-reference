using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HttpClients.Tests;

/// <summary>
/// Shared setup/teardown: boots the app with every outgoing HttpClient routed to a StubHandler
/// (so the real handler chain, base addresses and defaults still run) and disposes it afterwards.
/// </summary>
public abstract class HttpClientTestBase : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    protected HttpClientTestBase(params (string Key, string Value)[] settings)
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(settings.ToDictionary(s => s.Key, s => (string?)s.Value)));

            // Registered after the app's own defaults, so this primary handler is the one that wins.
            builder.ConfigureServices(services =>
                services.ConfigureHttpClientDefaults(http => http.ConfigurePrimaryHttpMessageHandler(() => Upstream)));
        });
        _client = _factory.CreateClient();
    }

    protected StubHandler Upstream { get; } = new();

    protected Task<HttpResponseMessage> GetAsync(string path) => _client.GetAsync(path);

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
