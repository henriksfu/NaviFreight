# NaviFreight

A multi-tenant SaaS platform for enterprise freight and logistics operations. NaviFreight gives dispatch teams, yard managers, and tenant administrators a unified view of fleet status, yard occupancy, route assignments, and operational alerts — all in real time.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 20 (standalone components, signals) |
| Backend | .NET 8 Minimal APIs |
| Database | SQL Server 2022 |
| Auth | JWT Bearer + SHA-256 password hashing |
| Styling | IBM Plex design tokens, custom SCSS |
| Local DB Runtime | Docker / Colima |

---

## Features

### Implemented

- **Authentication & RBAC** — JWT-based login with three roles: Tenant Admin, Dispatcher, Yard Manager. Role-gated UI actions and protected API routes.
- **Dashboard** — Live KPI band (active vehicles, yard occupancy, critical alerts, on-time routes, avg turnaround). Fleet status table and active alerts feed all pulled from real API data.
- **Fleet Management** — Full CRUD for vehicles and drivers. Assign/unassign drivers to vehicles. Status tracking (In Transit, At Dock, Awaiting Dispatch, Delayed).
- **Yard Management** — Yard list with capacity bars. Yard detail page with dock CRUD, inline assign/release vehicle per dock, operational status updates.
- **Route Dispatch** — Route list with expandable detail. Assign multiple vehicles to a route, unassign per vehicle. Create, update, and delete routes.
- **Alerts** — Status-tabbed alert feed (Active, Acknowledged, Resolved, Closed). One-click acknowledge, inline resolve with notes, reopen, reassign owner. Severity filtering (Critical, Warning, Info).
- **Multi-tenancy** — All API requests scoped to tenant via `X-Tenant-Id` header. SQL queries filter by `TenantId` throughout.
- **Real database** — SQL Server 2022 via Docker. Idempotent schema migrations, full demo seed data, 46 stored procedures covering all operations.

### In Progress / Planned

- **User Management** — Admin UI to invite, edit, and deactivate users within a tenant.
- **Settings Page** — Currently a static placeholder. Planned: tenant branding, notification preferences, API key management.
- **Reports Page** — Currently shows seeded snapshots. Planned: date-range filters, CSV export, charts.
- **Global Error Handling** — HTTP interceptor for 401/403/5xx with user-facing toasts.
- **Real-time Updates** — SignalR integration for live fleet position and alert push.
- **Pagination** — Fleet, routes, and alerts are currently unbounded lists.
- **Password Security Upgrade** — SHA-256 is dev-only. Production requires bcrypt or Argon2.
- **Azure Deployment** — Infrastructure scaffolding exists in `infra/azure/`. App Service + Azure SQL deployment not yet wired.
- **End-to-End Tests** — Playwright test suite planned.
- **CI/CD Pipeline** — GitHub Actions workflows for build, test, and deploy.

---

## Project Structure

```
NaviFreight/
├── frontend/                  # Angular 20 application
│   └── src/app/
│       ├── core/              # Services, models, guards, interceptors
│       ├── features/          # Page components (dashboard, fleet, yards, routes, alerts)
│       ├── layout/            # App shell and sidebar
│       └── shared/            # Reusable components (KPI card, etc.)
├── backend/
│   └── src/NaviFreight.Api/
│       ├── Configuration/     # Options classes (JWT, Tenant, DataAccess)
│       ├── Contracts/         # API response records
│       ├── Endpoints/         # Minimal API route handlers
│       ├── Middleware/        # Tenant context middleware
│       ├── Models/            # Request models
│       ├── Repositories/      # SQL data access (IAuthRepository, IOperationsRepository)
│       └── Services/          # Business logic (auth, operations) — SQL + InMemory impls
├── database/
│   ├── schema/                # 5 idempotent migration scripts
│   ├── seeds/                 # Demo data for all entities
│   ├── stored-procedures/     # 46 stored procedures
│   ├── init.sql               # Master script (runs schema + seeds + procs)
│   └── setup-local.sh         # One-command local DB setup
├── docker-compose.yml         # SQL Server 2022 container
├── docs/                      # Architecture, standards, implementation plan
└── infra/azure/               # Azure environment scaffolding
```

---

## Local Setup

### Prerequisites

- Node.js 20+
- .NET 8 SDK
- Homebrew (macOS) — for Docker/Colima if not already installed

### 1. Start the database

```bash
bash database/setup-local.sh
```

This installs Colima + Docker CLI if missing, pulls SQL Server 2022, starts the container, and applies the full schema, seeds, and stored procedures. Takes ~5 minutes on first run (image pull).

### 2. Start the backend

```bash
cd backend
dotnet run --project src/NaviFreight.Api
```

API runs on `http://localhost:5000`. Swagger UI available at `http://localhost:5000/swagger`.

### 3. Start the frontend

```bash
cd frontend
npm install
npm start
```

App runs on `http://localhost:4200`. API calls proxy to `http://localhost:5000` automatically.

---

## Demo Credentials

| Role | Email | Password |
|---|---|---|
| Tenant Admin | morgan.ellis@atlasmeridian.example | demo@Admin1 |
| Dispatcher | priya.shah@atlasmeridian.example | demo@Disp1 |
| Yard Manager | darius.cole@atlasmeridian.example | demo@Yard1 |

Demo tenant: **Atlas Meridian Logistics** (`tenant-demo`)

---

## API Overview

All endpoints require `Authorization: Bearer <token>` and `X-Tenant-Id` headers (except `/api/auth/login`).

| Method | Path | Description |
|---|---|---|
| POST | `/api/auth/login` | Login, returns JWT |
| GET | `/api/auth/me` | Current user profile |
| GET | `/api/fleet/vehicles` | List all vehicles |
| GET/POST | `/api/fleet/vehicles/{id}` | Vehicle detail / create |
| GET | `/api/fleet/drivers` | List all drivers |
| GET | `/api/yards` | List yards with occupancy |
| GET | `/api/yards/{id}` | Yard detail with docks |
| GET | `/api/routes` | List route assignments |
| GET | `/api/alerts` | List alerts (filter by status/severity) |
| GET | `/api/dashboard/summary` | KPI summary |
| GET | `/health` | Health check |

---

## Configuration

### Backend (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "NaviFreight": "Server=localhost;Database=NaviFreight;User Id=sa;Password=...;TrustServerCertificate=True;"
  },
  "DataAccess": { "Provider": "SqlServer" },
  "Jwt": {
    "Key": "<min-32-char-secret>",
    "Issuer": "navifreight-api",
    "Audience": "navifreight-app",
    "ExpiryMinutes": 480
  }
}
```

Switch `Provider` to `"InMemory"` to run without a database (uses built-in seed data).

> **Note:** The JWT key and SA password in `appsettings.json` are for local development only. Rotate both before any deployment.

---

## License

MIT
