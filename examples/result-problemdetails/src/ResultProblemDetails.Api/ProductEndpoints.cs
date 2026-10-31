namespace ResultProblemDetails.Api;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");

        group.MapGet("/{id:guid}", (Guid id, ProductService service) =>
            service.Get(id).ToResult(product => Results.Ok(product)));

        group.MapPost("/", (CreateProductRequest request, ProductService service) =>
            service.Create(request).ToResult(product =>
                Results.Created($"/products/{product.Id}", product)));

        return app;
    }
}
