# Exception handling with `IExceptionHandler`

> Chained `IExceptionHandler`s, `AddProblemDetails`, and status code pages. Targets .NET 10.

## Why it exists

Unexpected exceptions need one consistent, safe response: logged in full, shown to the client as `ProblemDetails` without internals. `IExceptionHandler` (since .NET 8) replaces hand-written exception middleware with a composable chain.

## Key code

- [`DomainExceptionHandler.cs`](src/ExceptionHandling.Api/DomainExceptionHandler.cs): handles one family of exceptions, returns `false` for everything else.
- [`GlobalExceptionHandler.cs`](src/ExceptionHandling.Api/GlobalExceptionHandler.cs): the catch-all, logs and returns a generic 500.
- [`ErrorHandlingExtensions.cs`](src/ExceptionHandling.Api/ErrorHandlingExtensions.cs): registration, order, and `CustomizeProblemDetails`.

```csharp
services.AddExceptionHandler<DomainExceptionHandler>();   // runs first
services.AddExceptionHandler<GlobalExceptionHandler>();   // runs last

app.UseExceptionHandler();
app.UseStatusCodePages();   // 404/405 with empty bodies become problem details too
```

## Run it

```bash
dotnet run --project src/ExceptionHandling.Api   # prints the URL it listens on
curl -i http://localhost:<port>/orders/42   # 404 from a domain exception
curl -i http://localhost:<port>/boom        # 500, no internals leaked
dotnet test
```

## When to use it (and when not to)

- Use it as the single safety net for unhandled exceptions.
- Registration order is execution order. A handler returning `true` stops the chain.
- Don't use exceptions for expected failures. See [`result-problemdetails`](../result-problemdetails).
- Pitfall: in Development, `UseDeveloperExceptionPage` shows full details. Test the production path too.

## Further reading

- [Handle errors in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)
