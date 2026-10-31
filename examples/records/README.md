# Records

> Positional records, `with`, value equality, record structs, hierarchies, and validated value objects. Targets .NET 10 / C# 14.

## Why it exists

Records give you immutable data types with value equality, `ToString`, deconstruction and non-destructive mutation for free. They suit DTOs, value objects, strongly typed IDs and closed hierarchies.

## Key code

- [`ValueObjects.cs`](src/Records.Api/ValueObjects.cs): `Money` (positional), `Email` (validated with C# 14 `field`), `OrderId` (`readonly record struct`), `Counter` (mutable record struct).
- [`Shapes.cs`](src/Records.Api/Shapes.cs): a record hierarchy with a `switch` expression.
- [`Orders.cs`](src/Records.Api/Orders.cs): request DTOs (positional) and a response DTO (`required` + `init`).

```csharp
var original = new Money(5m, "USD");
var changed  = original with { Amount = 10m };   // copy, original untouched
var (amount, currency) = changed;                // deconstruction
```

## Run it

```bash
dotnet run --project src/Records.Api   # prints the URL it listens on
dotnet test
```

## When to use it (and when not to)

- Use `record` (class) for DTOs and value objects. Use `readonly record struct` for small, hot-path values such as IDs.
- Gotcha: equality is shallow. Records holding a `List<T>` compare the list by reference.
- Gotcha: validation in a positional record's initializer does **not** run for `with`. Validate in the `init` accessor instead (see `Email` and the tests).
- Avoid records for EF Core entities that need identity and mutable tracking.

## Further reading

- [Records (C# reference)](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)
- [The `field` keyword](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/field)
