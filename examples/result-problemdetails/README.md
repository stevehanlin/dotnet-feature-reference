# Result pattern with ProblemDetails

> `ErrorOr<T>` results mapped to RFC 9457 `ProblemDetails`. Targets .NET 10.

## Why it exists

Not finding a product or rejecting invalid input is normal control flow, not an exceptional event. Returning errors as values keeps the happy path readable, avoids exception cost, and lets one mapper turn every failure into a consistent HTTP response.

## Key code

- [`ProductService.cs`](src/ResultProblemDetails.Api/ProductService.cs): returns `ErrorOr<Product>`, never throws for expected failures.
- [`ErrorOrExtensions.cs`](src/ResultProblemDetails.Api/ErrorOrExtensions.cs): `ErrorType` to status code mapping, `ValidationProblem` for validation errors.
- [`ProductEndpoints.cs`](src/ResultProblemDetails.Api/ProductEndpoints.cs): endpoints stay one-liners.

```csharp
group.MapGet("/{id:guid}", (Guid id, ProductService service) =>
    service.Get(id).ToResult(product => Results.Ok(product)));
```

| `ErrorType` | HTTP status |
|---|---|
| `Validation` | 400 (as `ValidationProblem` with an `errors` map) |
| `Unauthorized` / `Forbidden` | 401 / 403 |
| `NotFound` | 404 |
| `Conflict` | 409 |
| anything else | 500 |

## Run it

```bash
dotnet run --project src/ResultProblemDetails.Api   # prints the URL it listens on
curl -i http://localhost:<port>/products/00000000-0000-0000-0000-000000000000
dotnet test
```

## When to use it (and when not to)

- Use it for expected business failures that a caller can act on.
- Keep exceptions for the unexpected (bugs, I/O failures). See [`exception-handling`](../exception-handling) for the safety net.
- Pitfall: don't return an error per field without a stable `Code`; clients key off it.

## Further reading

- [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api)
- [ErrorOr on GitHub](https://github.com/amantinband/error-or)
- [RFC 9457: Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457)
