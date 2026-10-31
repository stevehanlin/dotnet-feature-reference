namespace ExceptionHandling.Api;

public static class DemoEndpoints
{
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id:int}", (int id) =>
            id == 1 ? Results.Ok(new { id, status = "shipped" }) : throw new OrderNotFoundException(id));

        app.MapGet("/quantity/{value:int}", (int value) =>
            value > 0 ? Results.Ok(value) : throw new InvalidQuantityException(value));

        app.MapGet("/boom", IResult () => throw new InvalidOperationException("secret internal detail"));

        return app;
    }
}
