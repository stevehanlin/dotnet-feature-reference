using System.Net.Http.Json;

namespace HttpClients.Api;

/// <summary>Approach 3: a typed client. A plain class that receives a pre-configured HttpClient.</summary>
public sealed class QuoteClient(HttpClient http)
{
    public Task<Quote?> GetAsync(int id) => http.GetFromJsonAsync<Quote>($"quotes/{id}");
}

/// <summary>Approach 4: a typed client behind an interface, so consumers can be unit tested with a fake.</summary>
public interface IQuoteGateway
{
    Task<Quote?> GetAsync(int id);
}

public sealed class QuoteGateway(HttpClient http) : IQuoteGateway
{
    public Task<Quote?> GetAsync(int id) => http.GetFromJsonAsync<Quote>($"quotes/{id}");
}
