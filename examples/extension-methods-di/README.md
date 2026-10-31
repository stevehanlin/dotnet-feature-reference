# Extension methods for clean startup

> `AddXxx` / `UseXxx` / `MapXxx` composition, plus C# 14 extension blocks and members. Targets .NET 10 / C# 14.

## Why it exists

A 200-line `Program.cs` hides what the app actually does. Moving each feature's wiring behind one extension method makes startup read like a table of contents and keeps registration details next to the code they register.

## Key code

- [`Program.cs`](src/ExtensionMethodsDi.Api/Program.cs): the whole startup in a handful of lines.
- [`ServiceCollectionExtensions.cs`](src/ExtensionMethodsDi.Api/ServiceCollectionExtensions.cs): classic `this` form, the C# 14 `extension(...)` block, an options lambda overload, a config-bound overload, and an extension **property**.
- [`WebApplicationExtensions.cs`](src/ExtensionMethodsDi.Api/WebApplicationExtensions.cs): `UseXxx` / `MapXxx`, and a static extension member.

```csharp
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddGreeter(Action<GreeterOptions>? configure = null) { ... }
        public bool HasGreeter => services.Any(d => d.ServiceType == typeof(IGreeter));
    }
}
```

## Run it

```bash
dotnet run --project src/ExtensionMethodsDi.Api   # prints the URL it listens on
curl http://localhost:<port>/greet/Ada
dotnet test
```

## When to use it (and when not to)

- Use one `AddXxx` per feature, using `TryAdd*` so tests and callers can override.
- Put the extension next to the feature it wires, not in a giant `Extensions` folder.
- The `extension(...)` block needs C# 14 (.NET 10 SDK). Classic `this` methods still work everywhere and are fine for single methods.
- Don't hide *business* behavior in extension methods; they are for wiring.

## Further reading

- [Extension members (C# 14)](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14#extension-members)
- [Dependency injection in .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)
