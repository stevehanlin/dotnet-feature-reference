using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Records.Api;

namespace Records.Tests;

public class RecordTests
{
    [Fact]
    public void Records_have_value_equality()
    {
        Assert.Equal(new Money(5m, "usd"), new Money(5m, "USD"));
        Assert.NotEqual(new Money(5m, "USD"), new Money(6m, "USD"));
    }

    [Fact]
    public void With_expression_copies_and_changes_one_property()
    {
        var original = new Money(5m, "USD");
        var changed = original with { Amount = 10m };

        Assert.Equal(5m, original.Amount);   // original untouched
        Assert.Equal(10m, changed.Amount);
        Assert.NotSame(original, changed);
    }

    [Fact]
    public void Positional_records_deconstruct()
    {
        var (amount, currency) = new Money(3m, "eur");

        Assert.Equal(3m, amount);
        Assert.Equal("EUR", currency);
    }

    [Fact]
    public void Gotcha_with_expression_bypasses_positional_initializer_validation()
    {
        var money = new Money(1m, "USD");

        Assert.Throws<ArgumentException>(() => new Money(1m, "DOLLARS"));
        Assert.Equal("DOLLARS", (money with { Currency = "DOLLARS" }).Currency);   // no exception
    }

    [Fact]
    public void Init_accessor_validation_holds_for_initializers_and_with_expressions()
    {
        var email = new Email { Value = "Ada@Example.com" };

        Assert.Equal("ada@example.com", email.Value);
        Assert.Throws<ArgumentException>(() => new Email { Value = "nope" });
        Assert.Throws<ArgumentException>(() => email with { Value = "nope" });
    }

    [Fact]
    public void Generated_ToString_lists_members()
    {
        Assert.Equal("Money { Amount = 5, Currency = USD }", new Money(5m, "USD").ToString());
    }

    [Fact]
    public void Record_struct_ids_compare_by_value()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(new OrderId(guid), new OrderId(guid));
    }

    [Fact]
    public void Mutable_record_struct_copies_on_assignment()
    {
        var a = new Counter(0);
        var b = a;
        b.Increment();

        Assert.Equal(0, a.Count);
        Assert.Equal(1, b.Count);
    }

    [Fact]
    public void Record_hierarchy_works_with_switch_expressions()
    {
        Assert.Equal(12, new Shape.Rectangle(3, 4).Area());
        Assert.Equal(Math.PI, new Shape.Circle(1).Area(), 5);
        Assert.Equal(6, new Shape.Triangle(3, 4).Area());
    }

    [Fact]
    public void Collections_inside_records_use_reference_equality()
    {
        // Gotcha: record equality is shallow. Two lists with the same items are not equal.
        var a = new CreateOrderRequest("x", [new("A", 1, 1m)]);
        var b = new CreateOrderRequest("x", [new("A", 1, 1m)]);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public async Task Records_round_trip_through_the_api_as_json()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/orders",
            new CreateOrderRequest("Ada", [new("A", 2, 10m), new("B", 1, 5m)]));

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(new Money(25m, "USD"), order!.Total);
        Assert.Equal("Ada", order.Customer);
    }
}
