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
| `WidgetDepot.ApiService` | ASP.NET Core API | Backend API and EF Core data layer |
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
│   │   ├── Components/                   # Shared Blazor components and layouts
│   │   ├── wwwroot/                      # Static assets
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   ├── WidgetDepot.ApiService/           # Backend API service
│   │   ├── Data/                         # EF Core DbContext and migrations
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
├── CatalogPage.razor.cs        # Code-behind
├── CatalogPage.razor.css       # CSS rules
├── CatalogService.cs           # Calls ApiService and handles UI logic for this slice
└── CatalogModels.cs            # DTOs and view models for this slice
```

Slices do not share services or models with each other. Each slice calls ApiService APIs independently and works with DTOs returned from the backend.

---

## Key Technology Decisions

| Concern | Choice |
|---|---|
| UI Framework | Blazor Web App (.NET 10) |
| CSS | Bootstrap 5.3 |
| Database | PostgreSQL via EF Core |
| Authentication | Individual authentication (cookies) |
| Local hosting | .NET Aspire |
| Logging / Tracing | OpenTelemetry + `ILogger<T>` |
| Testing | xUnit |
| Code style | `.editorconfig` |
| Design pattern | Vertical Slice Architecture |
| Mediator library | None (no MediatR) |

---

## Notes

- Web and ApiService are separate projects with clear separation: Web calls ApiService APIs and works with DTOs. Web does not directly reference the Data layer.
- No MediatR dependency. Feature handlers are plain C# classes called directly.
- Health check endpoints are included in the application.
- Trunk-based development on `main`.
- ERP integration uses file-based FTP export (daily batch) — no real-time API.
- Authentication is wired for Azure Entra in production but bypassed locally to simplify development.
