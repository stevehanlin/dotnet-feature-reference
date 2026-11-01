using Microsoft.Extensions.Options;

namespace HttpClients.Api;

/// <summary>
/// A DelegatingHandler: middleware for outgoing requests. Registered transient and attached
/// per client with AddHttpMessageHandler, so each client opts in to the cross-cutting behavior.
/// </summary>
public sealed class ApiKeyHandler(IOptions<QuotesOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Api-Key", options.Value.ApiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
