namespace ResultProblemDetails.Api;

public sealed record Product(Guid Id, string Name, decimal Price);

public sealed record CreateProductRequest(string Name, decimal Price);
