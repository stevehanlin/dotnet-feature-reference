using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HttpClients.Api;

public static class QuoteEndpoints
{
    public static IEndpointRouteBuilder MapQuoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/quotes");

        // 1. Unnamed client: no base address, so the URL must be absolute.
        group.MapGet("/basic/{id:int}", (int id, IHttpClientFactory factory, IOptions<QuotesOptions> options) =>
            Respond("basic", () => factory.CreateClient().GetFromJsonAsync<Quote>($"{options.Value.BaseUrl}/quotes/{id}")));

        // 2. Named client: base address, API key handler and defaults all come from the registration.
        group.MapGet("/named/{id:int}", (int id, IHttpClientFactory factory) =>
            Respond("named", () => factory.CreateClient(ClientNames.Named).GetFromJsonAsync<Quote>($"quotes/{id}")));

        // 3. Typed client, injected directly.
        group.MapGet("/typed/{id:int}", (int id, QuoteClient client) =>
            Respond("typed", () => client.GetAsync(id)));

        // 4. Typed client behind an interface.
        group.MapGet("/typed-interface/{id:int}", (int id, IQuoteGateway gateway) =>
            Respond("typed-interface", () => gateway.GetAsync(id)));

        // 5. Keyed HttpClient.
        group.MapGet("/keyed/{id:int}", (int id, [FromKeyedServices(ClientNames.Keyed)] HttpClient client) =>
            Respond("keyed", () => client.GetFromJsonAsync<Quote>($"quotes/{id}")));

        return app;
    }

    // Maps upstream failures to 502 so callers see "bad gateway", not an unhandled 500.
    private static async Task<IResult> Respond(string approach, Func<Task<Quote?>> fetch)
    {
        try
        {
            var quote = await fetch();
            return quote is null
                ? Results.Problem("Upstream returned an empty body.", statusCode: StatusCodes.Status502BadGateway)
                : Results.Ok(new QuoteResult(approach, quote));
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
