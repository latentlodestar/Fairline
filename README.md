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

- **`ingest`** — raw provider data: `providers`, `odds_records`, `ingest_runs`, `ingest_logs`, `provider_requests`, `events`, `odds_snapshots`, `provider_catalog_snapshots`, `sport_catalog`, `tracked_leagues`
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

## Secrets & Configuration

### Odds API Key (required for ingestion)

The ingestion system uses [The Odds API v4](https://the-odds-api.com/liveapi/guides/v4/) and requires an API key.

**Development setup (user-secrets):**

```bash
dotnet user-secrets init --project src/Fairline.Api
dotnet user-secrets set "OddsApi:ApiKey" "<YOUR_KEY>" --project src/Fairline.Api
```

**Container/production setup (environment variable):**

```bash
export ODDS_API_KEY=<YOUR_KEY>
```

The environment variable `ODDS_API_KEY` automatically maps to `OddsApi:ApiKey` in configuration.

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
| POST | `/api/ingest/catalog/refresh` | Refresh sports catalog from Odds API |
| GET | `/api/ingest/catalog` | Get sports catalog + tracked leagues |
| POST | `/api/ingest/catalog/track` | Toggle tracked league `{ providerSportKey, enabled }` |
| POST | `/api/ingest/run` | Start gap-fill ingestion `{ windowHours, regions, markets, books }` |
| GET | `/api/ingest/runs` | List recent ingestion runs |
| GET | `/api/ingest/runs/{runId}` | Get run detail with logs |
| GET | `/api/ingest/runs/{runId}/stream` | SSE stream of run progress (events: `log`, `progress`, `summary`) |
| GET | `/api/modeling/scenarios` | List scenarios |

## Ingestion Workflow

### 1. Set up your Odds API key

See [Secrets & Configuration](#secrets--configuration) above.

### 2. Refresh the sports catalog

Navigate to the **Ingestion** page in the UI, or call the API:

```bash
curl -X POST http://localhost:5192/api/ingest/catalog/refresh
```

This fetches all available sports/leagues from The Odds API and stores them in the `sport_catalog` table.

### 3. Enable leagues to track

In the UI, toggle the checkboxes for leagues you want to track (e.g., NFL, NBA, EPL). By default, **no leagues are enabled** — you must explicitly select which to track.

Or via API:
```bash
curl -X POST http://localhost:5192/api/ingest/catalog/track \
  -H "Content-Type: application/json" \
  -d '{"providerSportKey": "basketball_nba", "enabled": true}'
```

### 4. Run gap-fill ingestion

Click **Start Ingestion** in the UI to run a gap-fill ingestion. This will:
1. Check which tracked leagues need fresh data (based on freshness windows)
2. Call the Odds API for stale leagues
3. Store events and flattened odds snapshots
4. Stream progress via SSE to the UI

Default configuration:
- **Markets:** h2h, spreads, totals
- **Books:** DraftKings, Pinnacle (uses `bookmakers` param for efficiency)
- **Freshness windows:**
  - Events starting in ≤24h: 10-minute freshness
  - Events starting in 24–72h: 60-minute freshness
  - Events starting in >72h: 6-hour freshness

Or via API:
```bash
curl -X POST http://localhost:5192/api/ingest/run \
  -H "Content-Type: application/json" \
  -d '{"windowHours": 72, "regions": ["us"], "markets": ["h2h","spreads","totals"], "books": ["draftkings","pinnacle"]}'
```

### 5. View logs and run history

The **Ingestion** page shows live SSE logs during a run and a table of recent runs with status, counts, and errors.

## Assumptions

- **Outrights/futures:** Skipped in v1. The catalog stores `hasOutrights` so they can be added later.
- **Bookmaker filtering:** Uses the Odds API `bookmakers` parameter (overrides `regions`; 10 books = 1 region quota cost). Defaults to DraftKings + Pinnacle but configurable per run.
- **Provider:** Hard-coded to "the-odds-api" as the sole provider. The schema supports multiple providers but v1 only implements one.
- **Odds format:** Always `decimal`. Stored as `numeric(18,6)` in Postgres.
- **Gap detection:** Coarse-grained per league (not per-event). If any event in a league is stale, the entire league is refreshed. This slightly over-fetches but minimizes complexity.
- **Background execution:** Ingestion runs execute in a background `Task.Run` within the API process. No persistent job queue in v1.

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
