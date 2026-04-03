# Architecture: Widget Depot

This document describes the planned solution structure for the Widget Depot Blazor application.

---

## Solution Overview

The solution is a .NET 10 Blazor Web App using Vertical Slice Architecture. It is a single-page web application with a PostgreSQL database, hosted locally via .NET Aspire. Azure Entra provides authentication in production; authentication is disabled for local development.

---

## Projects

| Project | Type | Purpose |
|---|---|---|
| `WidgetDepot.Web` | Blazor Web App | Main application — UI, routing, and feature slices |
| `WidgetDepot.AppHost` | .NET Aspire AppHost | Local orchestration (database, app, services) |
| `WidgetDepot.ServiceDefaults` | .NET Aspire ServiceDefaults | Shared Aspire defaults (OpenTelemetry, health checks, service discovery) |
| `WidgetDepot.Tests` | xUnit Test Project | Unit and integration tests |

---

## Folder Structure

```
WidgetDepot.sln
│
├── src/
│   ├── WidgetDepot.Web/                  # Main Blazor application
│   │   ├── Features/                     # Vertical slices (one folder per feature)
│   │   │   ├── Catalog/
│   │   │   ├── Accounts/
│   │   │   ├── Orders/
│   │   │   ├── Admin/
│   │   │   └── ProblemReports/
│   │   ├── Data/                         # EF Core DbContext and migrations
│   │   ├── Components/                   # Shared Blazor components and layouts
│   │   ├── wwwroot/                      # Static assets
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   ├── WidgetDepot.AppHost/              # Aspire host for local development
│   │   └── Program.cs
│   │
│   └── WidgetDepot.ServiceDefaults/      # Shared Aspire service configuration
│       └── Extensions.cs
│
└── tests/
    └── WidgetDepot.Tests/                # xUnit tests
        └── Features/                     # Tests mirror the Features folder structure
```

---

## Vertical Slice Structure

Each feature slice lives entirely within its own folder under `Features/`. A typical slice contains:

```
Features/Catalog/
├── CatalogPage.razor           # Blazor page component
├── CatalogPage.razor.cs        # Code-behind (if needed)
├── CatalogService.cs           # Business logic / data access for this slice
└── CatalogModels.cs            # DTOs, view models, form models
```

Slices do not share services or models with each other except through the shared `Data/` layer (EF Core entities and DbContext).

---

## Key Technology Decisions

| Concern | Choice |
|---|---|
| UI Framework | Blazor Web App (.NET 10) |
| CSS | Bootstrap 5.3 |
| Database | PostgreSQL via EF Core |
| Authentication | Azure Entra (OIDC); disabled locally |
| Local hosting | .NET Aspire |
| Logging / Tracing | OpenTelemetry + `ILogger<T>` |
| Testing | xUnit |
| Code style | `.editorconfig` |
| Design pattern | Vertical Slice Architecture |
| Mediator library | None (no MediatR) |

---

## Notes

- No MediatR dependency. Feature handlers are plain C# classes called directly.
- Health check endpoints are included in the application.
- Trunk-based development on `main`.
- ERP integration uses file-based FTP export (daily batch) — no real-time API.
- Authentication is wired for Azure Entra in production but bypassed locally to simplify development.
