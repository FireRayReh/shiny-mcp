# .NET / C# Best Practices

Condensed checklist for reviewing or writing .NET/C# code.

## Design & architecture
- Prefer primary constructors for dependency injection (`class MyClass(IDependency dependency)`).
- Use interface segregation; prefix interfaces with `I`.
- Apply the Factory pattern for complex object construction.
- Register services with the correct lifetime (Singleton, Scoped, Transient) via `Microsoft.Extensions.DependencyInjection`.
- Null-check constructor dependencies with `ArgumentNullException.ThrowIfNull`.

## Async
- Use `async`/`await` for all I/O and long-running work; return `Task`/`Task<T>`, never `async void` except event handlers.
- Use `ConfigureAwait(false)` in library code.
- Propagate `CancellationToken` through async call chains.

## Modernization (C# 12+/.NET 8+)
- Use collection expressions (`[]`), pattern matching, and switch expressions.
- Replace obsolete APIs; enable and honor nullable reference types.
- Use `Span<T>`/`Memory<T>` and `StringBuilder` to cut allocations on hot paths.

## Testing
- Follow Arrange-Act-Assert. Cover success, failure, and null/edge-case inputs.
- Prefer xUnit or MSTest with FluentAssertions; use Moq/NSubstitute for mocking.

## Configuration & logging
- Bind strongly-typed options via `IConfiguration` + data annotations validation.
- Use structured logging (`Microsoft.Extensions.Logging`) with scoped context; never log secrets.

## Security & performance
- Validate and sanitize all external input; use parameterized queries.
- Favor managed identity over stored credentials for Azure resource access.

Source: adapted from `github/awesome-copilot` (`skills/dotnet-best-practices`, `agents/expert-dotnet-software-engineer.agent.md`), MIT licensed.
