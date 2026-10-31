# Custom `TimeProvider`

> A `TimeProvider` subclass that reports a fixed business time zone, tested with `FakeTimeProvider`. Targets .NET 10.

## Why it exists

`DateTime.Now` and `DateTime.UtcNow` cannot be controlled in tests, and "local time" means whatever zone the server happens to run in. `TimeProvider` (since .NET 8) abstracts the clock. A custom subclass lets the app define what "local" means (a store's business zone) while tests control the passage of time without sleeping.

## Key code

- [`ZonedTimeProvider.cs`](src/CustomTimeProvider.Api/ZonedTimeProvider.cs): overrides `LocalTimeZone` and delegates everything else to an inner provider.
- [`StoreServices.cs`](src/CustomTimeProvider.Api/StoreServices.cs): `StoreHours` uses `GetLocalNow()`; `Heartbeat` uses `CreateTimer`.
- [`ServiceCollectionExtensions.cs`](src/CustomTimeProvider.Api/ServiceCollectionExtensions.cs): `AddBusinessClock("America/New_York")`.
- [`ZonedTimeProviderTests.cs`](tests/CustomTimeProvider.Tests/ZonedTimeProviderTests.cs): `FakeTimeProvider` as the inner clock.

```csharp
public sealed class ZonedTimeProvider(TimeProvider inner, TimeZoneInfo zone) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => zone;
    public override DateTimeOffset GetUtcNow() => inner.GetUtcNow();
    public override long GetTimestamp() => inner.GetTimestamp();
    public override long TimestampFrequency => inner.TimestampFrequency;
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
        inner.CreateTimer(callback, state, dueTime, period);
}
```

```csharp
var fake = new FakeTimeProvider(DateTimeOffset.Parse("2026-01-05T14:30:00Z"));
var clock = new ZonedTimeProvider(fake, newYork);   // 09:30 local
fake.Advance(TimeSpan.FromSeconds(90));             // fires timers, no sleeping
```

## Run it

```bash
dotnet run --project src/CustomTimeProvider.Api   # prints the URL it listens on
curl http://localhost:<port>/store/status
dotnet test
```

## When to use it (and when not to)

- Inject `TimeProvider` everywhere instead of calling `DateTime.UtcNow`. Register `TimeProvider.System` by default.
- Subclass only when you need different *behavior* (a fixed zone, a skewed clock). For tests alone, `FakeTimeProvider` is enough.
- Delegate all four members (UTC now, timestamp, frequency, timers) to the inner provider. Forgetting `CreateTimer` means `FakeTimeProvider.Advance()` never fires your timers, and forgetting `GetTimestamp` breaks `GetElapsedTime`.
- `GetLocalNow()` is not virtual. It is built from `GetUtcNow()` and `LocalTimeZone`, so override the zone rather than the method.
- Store and transmit UTC. Use local time only for presentation and business rules like opening hours.

## Further reading

- [What is the TimeProvider class?](https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview)
- [`FakeTimeProvider`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.time.testing.faketimeprovider)
