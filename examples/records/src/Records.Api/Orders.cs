namespace Records.Api;

/// <summary>Request DTO: immutable, validated by the type system where possible.</summary>
public sealed record CreateOrderRequest(string Customer, List<OrderLineRequest> Lines);

public sealed record OrderLineRequest(string Sku, int Quantity, decimal UnitPrice);

/// <summary>Response DTO. 'required' + 'init' is the alternative to a positional constructor.</summary>
public sealed record OrderResponse
{
    public required string Id { get; init; }
    public required string Customer { get; init; }
    public required Money Total { get; init; }
    public required IReadOnlyList<OrderLineRequest> Lines { get; init; }
}
