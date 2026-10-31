using Records.Api;

namespace Records.Tests;

/// <summary>Shared builders so each test states only what matters to it.</summary>
internal static class TestData
{
    public static Money Usd(decimal amount) => new(amount, "USD");

    public static OrderLineRequest Line(string sku = "A", int quantity = 1, decimal unitPrice = 1m) =>
        new(sku, quantity, unitPrice);

    public static CreateOrderRequest Order(string customer = "Ada", params OrderLineRequest[] lines) =>
        new(customer, [.. lines]);
}
