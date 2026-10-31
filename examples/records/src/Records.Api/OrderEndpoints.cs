namespace Records.Api;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", (CreateOrderRequest request) =>
        {
            var total = request.Lines
                .Select(l => new Money(l.Quantity * l.UnitPrice, "USD"))
                .Aggregate(Money.Zero("USD"), (sum, m) => sum + m);

            return Results.Ok(new OrderResponse
            {
                Id = OrderId.New().ToString(),
                Customer = request.Customer,
                Total = total,
                Lines = request.Lines,
            });
        });

        return app;
    }
}
