# HttpClient with IHttpClientFactory

> Every way to get an `HttpClient` from `IHttpClientFactory`, side by side, calling the same imaginary upstream. Targets .NET 10 / C# 14.

## Why it exists

`new HttpClient()` per request exhausts sockets; one static `HttpClient` never sees DNS changes. `IHttpClientFactory` pools and rotates the underlying `HttpMessageHandler`s for you, and gives you five different ways to consume that. They differ in how the client is configured, resolved and tested, so the right choice depends on the call site.

## Key code

- [`HttpClientExtensions.cs`](src/HttpClients.Api/HttpClientExtensions.cs): every registration, one per approach, plus `ConfigureHttpClientDefaults`.
- [`QuoteEndpoints.cs`](src/HttpClients.Api/QuoteEndpoints.cs): one endpoint per approach showing how each is consumed.
- [`Clients.cs`](src/HttpClients.Api/Clients.cs): the typed clients.
- [`ApiKeyHandler.cs`](src/HttpClients.Api/ApiKeyHandler.cs): a `DelegatingHandler` (outgoing middleware).
- [`HttpClientTestBase.cs`](tests/HttpClients.Tests/HttpClientTestBase.cs): routes every client to a `StubHandler` while the real handler chain still runs.

## The approaches

| # | Approach | Registered with | Consumed with |
| --- | --- | --- | --- |
| 1 | Basic (unnamed) | nothing, or `ConfigureHttpClientDefaults` | `factory.CreateClient()` |
| 2 | Named | `AddHttpClient("quotes", ...)` | `factory.CreateClient("quotes")` |
| 3 | Typed | `AddHttpClient<QuoteClient>(...)` | inject `QuoteClient` |
| 4 | Typed + interface | `AddHttpClient<IQuoteGateway, QuoteGateway>(...)` | inject `IQuoteGateway` |
| 5 | Keyed (.NET 9+) | `AddHttpClient("quotes-keyed", ...).AddAsKeyed()` | `[FromKeyedServices("quotes-keyed")] HttpClient` |

Two features apply across all of them:

- **Defaults:** `ConfigureHttpClientDefaults` configures every client, including unnamed ones (here: a `User-Agent` and a `SocketsHttpHandler` with `PooledConnectionLifetime`).
- **Handlers:** `AddHttpMessageHandler<T>()` adds a `DelegatingHandler` per client (here: `X-Api-Key`). Use it for auth, logging, retries and correlation ids. The unnamed client deliberately skips it, and the tests prove that.

### 1. Basic

```csharp
var client = factory.CreateClient();
var quote = await client.GetFromJsonAsync<Quote>($"{baseUrl}/quotes/{id}");
```

| Pros | Cons |
| --- | --- |
| Zero registration; the quickest way to get pooling and rotation | No base address, so every call repeats the absolute URL |
| Picks up `ConfigureHttpClientDefaults` | Can't attach per-client handlers (no auth, no retries) |
| Fine for one-off calls to arbitrary URLs | Configuration is scattered across call sites |

**Use for:** prototypes and calls to URLs you only know at runtime.

### 2. Named

```csharp
services.AddHttpClient("quotes", ConfigureQuotes).AddHttpMessageHandler<ApiKeyHandler>();
var client = factory.CreateClient("quotes");
```

| Pros | Cons |
| --- | --- |
| Base address, headers and handlers configured once | Magic string; a typo fails at runtime (use a constant, as here) |
| Several consumers can share one configuration | You still get a raw `HttpClient`: URLs and (de)serialization live in each caller |
| Easy to add more named clients for the same service (e.g. read vs write) | Needs `IHttpClientFactory` injected into the consumer |

**Use for:** a configured client shared by many small call sites, or several variants of one service.

### 3. Typed

```csharp
services.AddHttpClient<QuoteClient>(ConfigureQuotes);
public sealed class QuoteClient(HttpClient http) { ... }   // inject QuoteClient
```

| Pros | Cons |
| --- | --- |
| Strongly typed API: URLs, serialization and error handling live in one class | Registered **transient**: injecting it into a singleton captures one handler forever, so you lose handler rotation and DNS updates |
| No strings; discoverable and refactor-safe | Concrete class is harder to fake in consumer tests |
| `HttpClient` is injected already configured | One typed client per service (you can still name it with `AddHttpClient<T>("name")`) |

**Use for:** most service-to-service calls. This is the recommended default.

### 4. Typed + interface

```csharp
services.AddHttpClient<IQuoteGateway, QuoteGateway>(ConfigureQuotes);
```

| Pros | Cons |
| --- | --- |
| Everything from typed, plus consumers depend on an abstraction | An interface per client is extra ceremony |
| Trivially mockable in unit tests (no `HttpMessageHandler` stubbing) | Same transient / singleton-capture caveat as typed |
| Lets you swap implementations (e.g. a cache decorator) | |

**Use for:** clients with many consumers, or consumers you want to unit test without HTTP.

### 5. Keyed (.NET 9+)

```csharp
services.AddHttpClient("quotes-keyed", ConfigureQuotes).AddAsKeyed();
app.MapGet("/", ([FromKeyedServices("quotes-keyed")] HttpClient client) => ...);
```

| Pros | Cons |
| --- | --- |
| Named-client configuration with plain constructor/parameter injection | Still a magic string key and still a raw `HttpClient` |
| No `IHttpClientFactory` in the consumer | Newer API; less known, needs .NET 9+ |
| Works anywhere keyed services work | Same lifetime caveat: keep it out of singletons |

**Use for:** a named client you want injected directly without going through the factory.

### At a glance

| | Base address | Handlers | Typed API | Easy to fake | Magic string |
| --- | :---: | :---: | :---: | :---: | :---: |
| Basic | no | defaults only | no | medium | no |
| Named | yes | yes | no | medium | yes |
| Typed | yes | yes | yes | medium | no |
| Typed + interface | yes | yes | yes | easy | no |
| Keyed | yes | yes | no | medium | yes |

## Gotchas

- **Don't `new HttpClient()` per request.** It leaks sockets (`TIME_WAIT`). **Don't keep one static instance either** unless you set `SocketsHttpHandler.PooledConnectionLifetime`, or it never sees DNS changes.
- **Don't dispose clients from the factory.** Disposing an `HttpClient` is harmless (the handler is pooled), but you don't need to.
- **Typed and keyed clients are transient.** Injecting them into a singleton pins one handler for the app's life. In singletons, inject `IHttpClientFactory` and call `CreateClient` each time.
- **Handler lifetime defaults to 2 minutes** (`SetHandlerLifetime` changes it). That bounds how long a stale DNS entry can live.
- **Handler order matters.** Handlers run in the order added; the first is outermost.
- **Test with a fake primary handler,** not a fake `HttpClient`. `ConfigureHttpClientDefaults(h => h.ConfigurePrimaryHttpMessageHandler(...))` keeps your real handlers, base addresses and defaults under test.
- **Retries and circuit breakers** belong in `AddStandardResilienceHandler()` from `Microsoft.Extensions.Http.Resilience`, not hand-written handlers.

## Run it

The upstream host is fictional, so calls fail with `502` outside the tests. Point `Quotes:BaseUrl` at a real service to try it.

```bash
dotnet test
dotnet run --project src/HttpClients.Api   # prints the URL it listens on
curl http://localhost:<port>/quotes/typed/1
```

## Further reading

- [IHttpClientFactory with .NET](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory)
- [Guidelines for using HttpClient](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Keyed services](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#keyed-services)
