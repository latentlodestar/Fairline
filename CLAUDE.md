# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Dev Commands

```bash
# Development (Aspire — provisions Postgres, runs migrations, starts API + frontend)
./scripts/dev.sh

# Run all tests (.NET + frontend)
./scripts/test.sh

# .NET tests only (requires Docker for Testcontainers)
dotnet test Fairline.sln

# Single .NET test project
dotnet test tests/Fairline.Domain.Tests

# Single .NET test by name
dotnet test tests/Fairline.Domain.Tests --filter "FullyQualifiedName~MyTestMethod"

# Frontend tests
cd src/Fairline.Web && npm test

# Full solution build
dotnet build Fairline.sln

# Add EF Core migration (substitute context/schema as needed)
dotnet ef migrations add <Name> --context IngestDbContext --output-dir Migrations/Ingest --project src/Fairline.Infrastructure --startup-project src/Fairline.Api
dotnet ef migrations add <Name> --context ModelingDbContext --output-dir Migrations/Modeling --project src/Fairline.Infrastructure --startup-project src/Fairline.Api
```

## Architecture

.NET 10 monorepo with React frontend. Layered architecture with DDD-influenced bounded contexts.

### Layer dependency rules

| Layer              | May depend on              | Must NOT depend on       |
|--------------------|----------------------------|--------------------------|
| **Domain**         | Nothing                    | EF Core, HTTP, IO        |
| **Abstractions**   | Nothing                    | Any implementation       |
| **Application**    | Domain, Abstractions       | Infrastructure, API      |
| **Infrastructure** | Domain, Abstractions       | API                      |
| **Api**            | Application, Infrastructure (DI only) | Domain internals |

### Key patterns

- **Minimal APIs** with extension methods for route mapping (e.g., `MapStatusEndpoints`)
- **CQRS-style handlers** in Application layer (e.g., `GetStatusHandler`, `GetProvidersHandler`)
- **Two bounded contexts** with separate DbContexts: `IngestDbContext` (schema `ingest`) and `ModelingDbContext` (schema `modeling`), each with independent migration histories
- **Repository pattern** for data access (`IOddsRepository`, `IScenarioRepository`)
- **DependencyInjection.cs** files at each layer root wire up that layer's services
- **Migrator sidecar** (`Fairline.Migrator`) runs migrations on startup before API starts
- **Aspire orchestration** (`Fairline.AppHost`) provisions Postgres and coordinates startup ordering

### Frontend

React 19 + TypeScript 5.7 + Vite 6. API client in `src/Fairline.Web/src/api/client.ts`. Vite proxies `/api` and `/health` to the backend.

### Testing

- .NET: xUnit v3, AwesomeAssertions, NSubstitute, Testcontainers (Postgres)
- Frontend: Vitest, React Testing Library, jsdom
- Infrastructure tests use real Postgres via Testcontainers
- API tests use `WebApplicationFactory` for integration testing

### Package management

Centralized versioning in `Directory.Packages.props` — specify package versions there, not in individual `.csproj` files.
