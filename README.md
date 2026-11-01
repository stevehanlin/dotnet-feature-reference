# .NET feature reference

A personal cookbook of small, runnable .NET 10 examples, one feature per folder.

Every folder under [`examples/`](examples) is **independent**: it has its own solution, its own `global.json` and its own package versions, so you can copy one into another codebase and it builds. Each includes tests that prove what its README claims.

## Index

| Feature | Summary | Folder |
| --- | --- | --- |
| Options | `IOptions` / `IOptionsSnapshot` / `IOptionsMonitor`, validation on start | [examples/options](examples/options) |
| Result + ProblemDetails | `ErrorOr<T>` mapped to RFC 9457 problem responses | [examples/result-problemdetails](examples/result-problemdetails) |
| Exception handling | Chained `IExceptionHandler`, `AddProblemDetails`, status code pages | [examples/exception-handling](examples/exception-handling) |
| Records | Positional records, `with`, record structs, hierarchies, gotchas | [examples/records](examples/records) |
| Extension methods for DI | `AddXxx` composition and C# 14 extension members | [examples/extension-methods-di](examples/extension-methods-di) |
| Custom TimeProvider | A `TimeProvider` subclass with a fixed business time zone, tested with `FakeTimeProvider` | [examples/custom-time-provider](examples/custom-time-provider) |
| HttpClient factory | Basic, named, typed, typed + interface and keyed clients, delegating handlers, pros/cons | [examples/http-client-factory](examples/http-client-factory) |

## Using an example

```bash
cd examples/options
dotnet test
dotnet run --project src/Options.Api
```

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Code coverage

Coverage is collected and reported entirely inside GitHub Actions: no third-party service, account or token.

### Reading it in CI

Open any run of the **ci** workflow:

- **Job summary:** each example's job shows a coverage table (line and branch coverage per class) on the run's summary page.
- **Artifact:** download `coverage-<example>` from the run for the full HTML report (drill down to line-by-line highlighting). Artifacts are kept for 14 days.

### Running it locally

```bash
# one-time install
dotnet tool install --global dotnet-reportgenerator-globaltool

cd examples/options
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:coverage -reporttypes:"Html;TextSummary" -assemblyfilters:"-*.Tests"
open coverage/index.html   # macOS; use xdg-open on Linux
```

`TestResults/` and `coverage/` are git-ignored.

### How it is built (copy this into a new project)

1. **Collector:** the `dotnet new xunit` template already references `coverlet.collector`. Keep it in every test project.
2. **Collect:** run tests with `--collect:"XPlat Code Coverage"`. This writes a `coverage.cobertura.xml` per test project.
3. **Report:** [ReportGenerator](https://github.com/danielpalme/ReportGenerator) merges the Cobertura files. `MarkdownSummaryGithub` produces the job-summary table, `Html` produces the downloadable report, and `-assemblyfilters:"-*.Tests"` excludes the tests themselves from the numbers.
4. **Publish:** append the markdown to `$GITHUB_STEP_SUMMARY` and upload the HTML with `actions/upload-artifact`.

The relevant steps from [`.github/workflows/ci.yml`](.github/workflows/ci.yml):

```yaml
- name: Test with coverage
  run: >
    dotnet test "${{ matrix.solution }}" --nologo
    --collect:"XPlat Code Coverage"
    --results-directory "$RUNNER_TEMP/TestResults"
- name: Build coverage report
  run: |
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.4.1
    reportgenerator -reports:"$RUNNER_TEMP/TestResults/**/coverage.cobertura.xml" \
      -targetdir:coverage -reporttypes:"MarkdownSummaryGithub;Html" -assemblyfilters:"-*.Tests"
- name: Publish coverage summary
  run: cat coverage/SummaryGithub.md >> "$GITHUB_STEP_SUMMARY"
- uses: actions/upload-artifact@v4
  with: { name: coverage, path: coverage, retention-days: 14 }
```

Notes:

- Pin the ReportGenerator version so a tool update can't change your reports unexpectedly.
- To fail the build under a threshold, add `-Threshold` options via the `coverlet.msbuild` package (`/p:Threshold=80 /p:ThresholdType=line`). This repo only reports, it does not gate.
- A hosted service (Codecov, Coveralls) would add a README badge and PR comments. Swap step 4 for their upload action if you want that later.
- In this repo each example is its own solution, so the matrix reports each one separately. In a single-solution project the same steps run once.

## Adding an example

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)
