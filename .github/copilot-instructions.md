# GitHub Copilot Instructions

These instructions define how GitHub Copilot should assist with this project.
The goal is to ensure consistent, high-quality code generation aligned with our conventions, stack, and best practices.

The solution design exists in the `docs` folder. Make sure to update docs after significant changes and make sure that they reflect the current implementation.

For latest SDKs documentations, use Context 7.

## 🧠 Context

- **Project Type**: Web API / Console App / Microservice
- **Language**: C#
- **Framework**: .NET 8.0 / ASP.NET Core / .NET Aspire / Entity Framework Core
- **Libraries/Utilities**: xUnit / FluentAssertions / FluentValidation / FakeItEasy / Serilog / MediatR
- **Database**: SQL Server / PostgreSQL / SQLite
- **Architecture**: Clean Architecture / Microservices Architecture / Cloud-native Architecture / DDD / TDD / CQRS
- **Design Patterns**: Repository Pattern / Unit of Work / SOLID /Dependency Injection / Gang of Four Design Patterns
- **Deployment**: Docker / Kubernetes / Azure / GitOps / CI/CD
- **Authentication**: JWT / OAuth2 / OpenID Connect

## 🔧 General Guidelines

- Use C#-idiomatic patterns and follow .NET coding conventions.
- Use named methods instead of anonymous lambdas in business logic.
- Use nullable reference types (`#nullable enable`) and async/await.
- Format using `dotnet format` or IDE auto-formatting tools.
- Prioritize readability, testability, and SOLID principles.

## C# Conventions
- Use

- Use PascalCase for class, method, and property names.
- Use camelCase for local variables and method parameters.
- Use `var` for local variable declarations when the type is obvious.
- Use expression-bodied members for simple getters and methods.
- Use string interpolation (`$"Hello {name}"`) instead of `String.Format`.
- Use `using` statements for IDisposable objects.
- Use `Async` suffix for asynchronous methods.
- Use `I` prefix for interface names (e.g., `IRepository`).
- Use `nameof()` operator for referencing member names in exceptions and logging.
- Use primary contructor syntax for dependency injection.
- Use `record` types for immutable data structures.
- Use `switch` expressions for concise conditional logic.
- Use pattern matching for type checks and casts.
- Use `??` and `?.` operators for null handling.
- Use `throw` expressions for concise exception throwing.
- Import static classes when using their members to reduce verbosity.

## 🧶 Patterns

### ✅ Patterns to Follow
- Use Clean Architecture with layered separation.
- Use Dependency Injection for services and repositories.
- Use MediatR for CQRS (Commands/Queries).
- Use FluentValidation for input validation.
- Map DTOs to domain models using AutoMapper.
- Use ILogger<T> or Serilog for structured logging.
- For APIs:
  - Use [ApiController], ActionResult<T>, and ProducesResponseType.
  - Handle errors using middleware and Problem Details.

### 🚫 Patterns to Avoid
- Don’t use static state or service locators.
- Avoid logic in controllers—delegate to services/handlers.
- Don’t hardcode config—use appsettings.json and IOptions.
- Don’t expose entities directly in API responses.
- Avoid fat controllers and God classes.

## 🧪 Testing Guidelines
- Use xUnit for unit and integration testing.
- Use FakeItEasy for mocking dependencies.
- Follow Arrange-Act-Assert pattern in tests.
- Validate edge cases and exceptions.
- Prefer TDD for critical business logic and application services.

## 🧩 Example Prompts
- `Copilot, generate an ASP.NET Core controller with CRUD endpoints for Product.`
- `Copilot, implement a MediatR command handler for creating a new order.`
- `Copilot, create an Entity Framework Core DbContext for a blog application.`
- `Copilot, write an xUnit test for the CalculateInvoiceTotal method.`

## 🔁 Iteration & Review
- Copilot output should be reviewed and modified before committing.
- If code isn’t following these instructions, regenerate with more context or split the task.
- Use /// XML documentation comments to clarify intent for Copilot and future devs.
- Use Rider or Visual Studio code inspections to catch violations early.

## 📚 References
- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [xUnit Documentation](https://xunit.net/)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Serilog Docs](https://serilog.net/)
- [Clean Architecture in .NET (by Jason Taylor)](https://github.com/jasontaylordev/CleanArchitecture)