# Options pattern

> `IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>`, and startup validation. Targets .NET 10.

## Why it exists

Strongly typed configuration beats `configuration["Some:Key"]` scattered through the code. The options pattern binds a config section to a class, and validation makes a bad config fail at startup instead of on the first request.

## Key code

- [`ServiceCollectionExtensions.cs`](src/Options.Api/ServiceCollectionExtensions.cs): bind, validate, `ValidateOnStart()`.
- [`GreetingOptions.cs`](src/Options.Api/GreetingOptions.cs): data annotations on an options class.
- [`FeatureOptionsValidator.cs`](src/Options.Api/FeatureOptionsValidator.cs): `IValidateOptions<T>` for cross-property rules.
- [`OptionsEndpoints.cs`](src/Options.Api/OptionsEndpoints.cs): the three interfaces side by side.

```csharp
services.AddOptions<GreetingOptions>()
    .Bind(configuration.GetSection(GreetingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

| Interface | Lifetime | Sees config changes? |
|---|---|---|
| `IOptions<T>` | Singleton | No |
| `IOptionsSnapshot<T>` | Scoped | Yes, once per request |
| `IOptionsMonitor<T>` | Singleton | Yes, always current, with change callbacks |

## Run it

```bash
dotnet run --project src/Options.Api   # prints the URL it listens on
curl http://localhost:<port>/greeting/ioptions
dotnet test
```

## When to use it (and when not to)

- Use `IOptions<T>` by default. Use `IOptionsMonitor<T>` inside singletons that must react to changes. Use `IOptionsSnapshot<T>` in scoped code.
- `IOptionsSnapshot<T>` cannot be injected into singletons (it is scoped).
- Without `ValidateOnStart()`, validation only runs the first time the options are read.
- Don't use options for secrets management. Bind them from a secret store, don't invent one.

## Further reading

- [Options pattern in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)
