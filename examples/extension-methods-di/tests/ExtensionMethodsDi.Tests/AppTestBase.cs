using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace ExtensionMethodsDi.Tests;

/// <summary>
/// Shared setup/teardown: builds the app with optional overrides and disposes every
/// factory and client a test created.
/// </summary>
public abstract class AppTestBase : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    protected HttpClient CreateClient(Action<IWebHostBuilder>? configure = null)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => configure?.Invoke(builder));
        _disposables.Add(factory);

        var client = factory.CreateClient();
        _disposables.Add(client);
        return client;
    }

    /// <summary>Override: replaces the clock. Registered after the app's own registration, so it wins.</summary>
    protected static Action<IWebHostBuilder> WithClockAt(string utc) =>
        builder => builder.ConfigureServices(services =>
            services.AddSingleton<TimeProvider>(new FakeTimeProvider(DateTimeOffset.Parse(utc))));

    /// <summary>Override: sets one configuration value.</summary>
    protected static Action<IWebHostBuilder> WithSetting(string key, string value) =>
        builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?> { [key] = value }));

    public void Dispose()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();
    }
}
