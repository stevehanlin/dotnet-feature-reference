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
