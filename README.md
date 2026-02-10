# Fairline

Odds and edge analysis platform. Monorepo with .NET 10 backend, Aspire orchestration, Postgres storage, and React + Vite frontend.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.100+)
- [Node.js](https://nodejs.org/) 20+ and npm
- [Docker](https://www.docker.com/) (for Aspire resource provisioning and Docker Compose)
- [Aspire CLI](https://aspire.dev) (`dotnet tool install -g aspire`)

## Architecture

```
src/
  Fairline.AppHost/         Aspire orchestrator
  Fairline.ServiceDefaults/ Shared service config (OpenTelemetry, health, resilience)
  Fairline.Api/             Minimal API endpoints
  Fairline.Abstractions/    Interfaces + contracts/DTOs (shared between layers)
  Fairline.Application/     Use cases (commands/queries/handlers)
  Fairline.Domain/          Pure domain objects and value objects
  Fairline.Infrastructure/  EF Core, Postgres, repository implementations
  Fairline.Migrator/        Sidecar that runs EF Core migrations and exits
  Fairline.Web/             React + Vite + TypeScript frontend

tests/
  Fairline.Domain.Tests/
  Fairline.Application.Tests/
  Fairline.Infrastructure.Tests/   (uses Testcontainers for Postgres)
  Fairline.Api.Tests/              (integration tests with WebApplicationFactory)
```

### Layer Rules

| Layer | May depend on | Must NOT depend on |
|-------|--------------|-------------------|
| **Domain** | Nothing | EF Core, HTTP, IO |
| **Abstractions** | Nothing | Any implementation |
| **Application** | Domain, Abstractions | Infrastructure, API |
| **Infrastructure** | Domain, Abstractions, EF Core | API |
| **Api** | Application, Infrastructure (DI only) | Domain internals |

## Database

**Postgres** with two schemas:

- **`ingest`** — raw provider data: `providers`, `odds_records`
- **`modeling`** — scenario analysis: `scenarios`, `scenario_comparisons`

### Why separate DbContexts?

Each schema is owned by its own `DbContext` (`IngestDbContext`, `ModelingDbContext`). This provides:
- Independent migration histories per schema
- Clean bounded-context ownership
- Ability to evolve schemas independently

### Migrations

Migrations live in `src/Fairline.Infrastructure/Migrations/{Ingest,Modeling}/`.

The **Migrator** sidecar runs on startup, applies pending migrations for both contexts, then exits. In Aspire, the API uses `WaitForCompletion(migrator)` so it won't start until migrations finish.

To add a new migration:

```bash
# Ingest schema
dotnet ef migrations add <Name> \
  --context IngestDbContext \
  --output-dir Migrations/Ingest \
  --project src/Fairline.Infrastructure \
  --startup-project src/Fairline.Api

# Modeling schema
dotnet ef migrations add <Name> \
  --context ModelingDbContext \
  --output-dir Migrations/Modeling \
  --project src/Fairline.Infrastructure \
  --startup-project src/Fairline.Api
```

## Running

### With Aspire (recommended for dev)

```bash
./scripts/dev.sh
```

This starts the Aspire dashboard, provisions a Postgres container, runs the migrator, starts the API, and launches the frontend dev server. Open the Aspire dashboard URL shown in the terminal.

### With Docker Compose

```bash
./scripts/compose-up.sh
```

Starts Postgres, runs migrations, then starts the API on `http://localhost:5192`. Run the frontend separately:

```bash
cd src/Fairline.Web && npm install && npm run dev
```

### Frontend only

```bash
cd src/Fairline.Web
npm install
npm run dev
```

The Vite dev server proxies `/api` and `/health` requests to the API.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Health check (Aspire default) |
| GET | `/alive` | Liveness check |
| GET | `/api/status` | API version + DB connectivity |
| GET | `/api/ingest/providers` | List configured providers |
| GET | `/api/modeling/scenarios` | List scenarios |

## Testing

```bash
./scripts/test.sh
```

Or run backend and frontend tests separately:

```bash
# .NET tests (requires Docker for Testcontainers in Infrastructure tests)
dotnet test Fairline.sln

# Frontend tests
cd src/Fairline.Web && npm test
```

### Test stack

- **xUnit v3** + **AwesomeAssertions** + **NSubstitute** for .NET
- **Testcontainers** for Postgres integration tests
- **Vitest** + **React Testing Library** for frontend
