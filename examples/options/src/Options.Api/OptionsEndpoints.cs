using Microsoft.Extensions.Options;

namespace Options.Api;

public static class OptionsEndpoints
{
    public static IEndpointRouteBuilder MapOptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/greeting");

        // IOptions<T>: singleton, read once, never sees config changes.
        group.MapGet("/ioptions", (IOptions<GreetingOptions> options) => Render(options.Value));

        // IOptionsSnapshot<T>: scoped, re-read once per request.
        group.MapGet("/snapshot", (IOptionsSnapshot<GreetingOptions> options) => Render(options.Value));

        // IOptionsMonitor<T>: singleton, always current, can notify on change.
        group.MapGet("/monitor", (IOptionsMonitor<GreetingOptions> options) => Render(options.CurrentValue));

        app.MapGet("/features", (IOptions<FeatureOptions> options) => options.Value);

        return app;
    }

    private static string Render(GreetingOptions o) =>
        string.Join(' ', Enumerable.Repeat(o.Message, o.Repeat));
}
