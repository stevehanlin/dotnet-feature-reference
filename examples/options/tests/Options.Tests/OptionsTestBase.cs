using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Options.Tests;

/// <summary>
/// Shared setup/teardown: builds a host with in-memory configuration overrides and
/// disposes every factory and client a test created, even when startup throws.
/// </summary>
public abstract class OptionsTestBase : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    protected HttpClient CreateClient(params (string Key, string? Value)[] configOverrides)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(configOverrides.ToDictionary(o => o.Key, o => o.Value))));
        _disposables.Add(factory);   // registered before CreateClient, which is where startup validation throws

        var client = factory.CreateClient();
        _disposables.Add(client);
        return client;
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();
    }
}
