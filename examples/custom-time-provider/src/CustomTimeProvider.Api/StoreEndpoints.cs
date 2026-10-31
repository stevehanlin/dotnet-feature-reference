namespace CustomTimeProvider.Api;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/store/status", (StoreHours hours, TimeProvider clock) => new
        {
            isOpen = hours.IsOpen(),
            localTime = hours.LocalNow,
            zone = clock.LocalTimeZone.Id,
        });

        app.MapGet("/heartbeat", (Heartbeat heartbeat) => new { heartbeat.Beats });

        return app;
    }
}
