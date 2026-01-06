## Blutracker

Cross-platform media collection app (Movies + TV shows by season) built with **.NET MAUI**, with a storage abstraction so you can use:

- **Local SQLite** in development (offline-first)
- **Azure (staging/production)** via an HTTP API (Azure Functions + Cosmos DB)

### Repo layout

- `src/Dadstart.Labs.Blutracker.Core`: domain models + `IMediaRepository`
- `src/Dadstart.Labs.Blutracker.Infrastructure.Local`: SQLite implementation (`SqliteMediaRepository`)
- `src/Dadstart.Labs.Blutracker.Infrastructure.Azure`: HTTP implementation (`HttpMediaRepository`)
- `src/Dadstart.Labs.Blutracker.Functions`: Azure Functions API + Cosmos DB persistence
- `src/Dadstart.Labs.Blutracker.App`: .NET MAUI UI (Movies, TV shows, seasons)
- `tests/Dadstart.Labs.Blutracker.Tests`: unit tests for local persistence

### Running locally (persistence = SQLite)

The MAUI app uses SQLite when built as **Debug** (`#if DEBUG`). The database file lives in the platform app data directory as `blutracker.db`.

### Staging / production (persistence = Azure)

Non-Debug builds of the MAUI app use the Azure-backed repository. Configure these environment variables at runtime:

- `BLUTRACKER_API_BASEURI`: base URL of your Functions host (example: `https://your-app.azurewebsites.net/`)
- `BLUTRACKER_AUTH_MODE`: `FunctionKey` (default), `Bearer`, or `None`

If using **Function keys**:

- `BLUTRACKER_FUNCTION_KEY` (preferred) or `BLUTRACKER_API_KEY` (legacy)

If using **Entra ID / bearer tokens** (e.g. via App Service Authentication / EasyAuth):

- `BLUTRACKER_BEARER_TOKEN`

The Functions project uses:

- `CosmosConnectionString` (required)
- `CosmosDatabaseName` (optional, defaults to `Blutracker`)

`src/Dadstart.Labs.Blutracker.Functions/local.settings.json.example` shows a dev template (don’t commit real `local.settings.json`).

### Solutions

- `Dadstart.Labs.Blutracker.Backend.sln`: builds on Linux (core + infra + tests + functions)
- `Dadstart.Labs.Blutracker.App.sln`: MAUI app + dependencies (requires MAUI workloads installed)

### Build (this repo’s “backend build”)

From repo root:

```bash
dotnet build Dadstart.Labs.Blutracker.Backend.sln
dotnet test Dadstart.Labs.Blutracker.Backend.sln
```

### MAUI prerequisites (dev machine)

Install MAUI workloads (example):

```bash
dotnet workload install maui
```

Then open/build `Dadstart.Labs.Blutracker.App.sln`.

