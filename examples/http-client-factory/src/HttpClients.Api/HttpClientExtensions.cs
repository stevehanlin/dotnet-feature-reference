using Microsoft.Extensions.Options;

namespace HttpClients.Api;

public static class HttpClientExtensions
{
    public static IServiceCollection AddQuoteClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuotesOptions>(configuration.GetSection("Quotes"));
        services.AddTransient<ApiKeyHandler>();

        // Approach 1 setup: defaults apply to EVERY client, including the unnamed one.
        services.ConfigureHttpClientDefaults(http =>
        {
            http.ConfigureHttpClient(client => client.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-feature-reference/1.0"));

            // Recycle pooled connections so DNS changes are picked up (the factory also rotates handlers).
            http.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) });
        });

        // Approach 2: named client. Resolved by string through IHttpClientFactory.CreateClient(name).
        services.AddHttpClient(ClientNames.Named, ConfigureQuotes)
            .AddHttpMessageHandler<ApiKeyHandler>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Approach 3: typed client (concrete class).
        services.AddHttpClient<QuoteClient>(ConfigureQuotes)
            .AddHttpMessageHandler<ApiKeyHandler>();

        // Approach 4: typed client registered against an interface.
        services.AddHttpClient<IQuoteGateway, QuoteGateway>(ConfigureQuotes)
            .AddHttpMessageHandler<ApiKeyHandler>();

        // Approach 5: named client also exposed as a keyed HttpClient (.NET 9+).
        services.AddHttpClient(ClientNames.Keyed, ConfigureQuotes)
            .AddHttpMessageHandler<ApiKeyHandler>()
            .AddAsKeyed();

        return services;
    }

    // Shared by every configured client. Takes the IServiceProvider so it can read options lazily.
    private static void ConfigureQuotes(IServiceProvider services, HttpClient client) =>
        client.BaseAddress = new Uri(services.GetRequiredService<IOptions<QuotesOptions>>().Value.BaseUrl);
}
