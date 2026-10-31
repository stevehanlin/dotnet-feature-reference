# Adding an example

Each folder under `examples/` is **independent**: it can be copied out of this repo and still build. Do not share projects, `Directory.Build.props` or package versions between examples. Versions are allowed to drift.

## Steps

```bash
cd examples
mkdir my-feature && cd my-feature
dotnet new sln -n MyFeature
dotnet new web -n MyFeature.Api -o src/MyFeature.Api
dotnet new xunit -n MyFeature.Tests -o tests/MyFeature.Tests
dotnet sln add src/MyFeature.Api tests/MyFeature.Tests
dotnet add tests/MyFeature.Tests reference src/MyFeature.Api
dotnet add tests/MyFeature.Tests package Microsoft.AspNetCore.Mvc.Testing
```

Then:

1. Copy `global.json` and `.editorconfig` from an existing example.
2. Copy `_template/README.md` to the new folder and fill it in.
3. Keep `Program.cs` short: use `AddXxx` / `UseXxx` / `MapXxx` extension methods.
4. Add `public partial class Program;` so `WebApplicationFactory<Program>` works.
5. Write tests that prove the behavior the README claims.
6. Add a row to the index table in the root `README.md`.

CI discovers every `.sln` / `.slnx` under `examples/` automatically.

## Test conventions

- **Naming:** `Given<State>_When<Action>_Then<Outcome>`, for example `GivenUnknownId_WhenGettingProduct_ThenReturnsNotFoundProblemDetails`. Prefix a test with `Gotcha_` when it exists to document a surprising behavior.
- **Shared setup:** put setup and teardown in one `ApiTestBase` per test project (a `WebApplicationFactory<Program>` class fixture, a client per test disposed in `Dispose`, and request/parse helpers). Put test-data builders in a `TestData` class. Tests should state only what is specific to them.
- **Isolation:** if state is shared within a test class (an in-memory store), give each test its own data, such as a unique name, rather than relying on test order.
- **Coverage:** aim for 100% line and branch coverage of the example's own code. If a branch can't be reached through the host (for example `Activity.Current?.Id ?? fallback`), extract it into a small pure function and unit test it.

