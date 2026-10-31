# .NET feature reference

A personal cookbook of small, runnable .NET 10 examples, one feature per folder.

Every folder under [`examples/`](examples) is **independent**: it has its own solution, its own `global.json` and its own package versions, so you can copy one into another codebase and it builds. Each includes tests that prove what its README claims.

## Index

| Feature | Summary | Folder |
|---|---|---|
| Options | `IOptions` / `IOptionsSnapshot` / `IOptionsMonitor`, validation on start | [examples/options](examples/options) |
| Result + ProblemDetails | `ErrorOr<T>` mapped to RFC 9457 problem responses | [examples/result-problemdetails](examples/result-problemdetails) |
| Exception handling | Chained `IExceptionHandler`, `AddProblemDetails`, status code pages | [examples/exception-handling](examples/exception-handling) |
| Records | Positional records, `with`, record structs, hierarchies, gotchas | [examples/records](examples/records) |
| Extension methods for DI | `AddXxx` composition and C# 14 extension members | [examples/extension-methods-di](examples/extension-methods-di) |

## Using an example

```bash
cd examples/options
dotnet test
dotnet run --project src/Options.Api
```

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Adding an example

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)
