# Technology Stack for Widget Depot Website

## Architecture

- single-page application
- web-only, no mobile support
- external account management (OAuth/OIDC)
- "Vertical Slice Architecture" design patterns

## ASP.NET Blazor Application

- .NET 10 template (Blazor)
- Postgres SQL database
- Bootstrap 5.3 CSS Framework
- xUnit unit tests
- Azure Entra for user Authentication (disabled for local development)
- Use Aspire to host app locally

## Priorities

- Use EF Core for data access
- Use OpenTelemetry for logging and tracing
- Use editorconfig to enforce coding standards
- Use built-in ILogger<T> loggers when needed
- Trunk-based development (main branch)
- No dependency on MediatR NuGet package
- Include health check endpoints

