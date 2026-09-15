# C#/.NET Janitor Checklist

For cleanup, modernization, and tech-debt remediation passes. Work in this order, testing after each step:

1. **Compiler & analyzers** — resolve warnings and static-analysis diagnostics first; they hide real bugs.
2. **Deprecated/obsolete usage** — replace obsolete APIs with current alternatives.
3. **Dead code** — remove unused usings, variables, members, and unreachable branches.
4. **Modernize syntax** — pattern matching, switch expressions, collection expressions, primary constructors, nullable reference types.
5. **Performance** — `StringBuilder` over string concatenation in loops, correct `async`/`await` usage, reduce boxing/allocations, consider `Span<T>`.
6. **Test coverage** — add missing unit tests for public APIs (AAA pattern, FluentAssertions), integration tests for critical flows.
7. **Documentation** — XML doc comments on public members; update README/inline docs.

Rules: make small, incremental, behavior-preserving changes; run the test suite after every change; do not refactor and change behavior in the same step.

Source: adapted from `github/awesome-copilot` (`agents/csharp-dotnet-janitor.agent.md`), MIT licensed.
