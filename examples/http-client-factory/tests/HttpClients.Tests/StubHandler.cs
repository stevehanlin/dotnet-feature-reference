using System.Net;
using System.Text;

namespace HttpClients.Tests;

/// <summary>
/// Stands in for the network. Records every request that reaches the "wire" (after all
/// DelegatingHandlers ran) and answers like the quotes service would.
/// Special ids: 404 returns Not Found, 0 returns a JSON null body.
/// </summary>
public sealed class StubHandler : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _requests = [];

    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    public HttpRequestMessage Single => Assert.Single(_requests);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Add(request);
        var id = int.Parse(request.RequestUri!.Segments[^1]);

        return Task.FromResult(id switch
        {
            404 => new HttpResponseMessage(HttpStatusCode.NotFound),
            0 => Json("null"),
            _ => Json($$"""{"id":{{id}},"text":"quote {{id}}"}"""),
        });
    }

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
}
