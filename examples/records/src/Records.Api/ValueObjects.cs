namespace Records.Api;

/// <summary>Positional record: constructor, init properties, Deconstruct, equality, ToString, all generated.</summary>
public sealed record Money(decimal Amount, string Currency)
{
    // Validates construction only. GOTCHA: `money with { Currency = "bad" }` sets the
    // property directly and skips this initializer. See Email for validation that holds.
    public string Currency { get; init; } = !string.IsNullOrWhiteSpace(Currency) && Currency.Length == 3
        ? Currency.ToUpperInvariant()
        : throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(Currency));

    public static Money Zero(string currency) => new(0m, currency);

    public static Money operator +(Money a, Money b) =>
        a.Currency == b.Currency
            ? a with { Amount = a.Amount + b.Amount }
            : throw new InvalidOperationException("Cannot add different currencies.");
}

/// <summary>
/// Validation in the init accessor (C# 14 `field` keyword) runs for object initializers
/// and `with` expressions alike, so the invariant cannot be bypassed.
/// </summary>
public sealed record Email
{
    public required string Value
    {
        get;
        init => field = value.Contains('@') && !value.StartsWith('@')
            ? value.ToLowerInvariant()
            : throw new ArgumentException("Not a valid email address.", nameof(value));
    }
}

/// <summary>Strongly typed id: a record struct is a value type, so no heap allocation.</summary>
public readonly record struct OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

/// <summary>Without `readonly`, a record struct is mutable and copied on assignment.</summary>
public record struct Counter(int Count)
{
    public void Increment() => Count++;
}
