# HR Portal

Modern, open-source, self-hosted, multi-tenant HR platform built as a **Modular Monolith**.

## Architecture

```
┌─────────────┐     ┌─────────────┐     ┌──────────────┐
│   Nginx     │────▶│  Frontend   │     │   Keycloak   │
│  (reverse   │     │  React/Vite │     │  (identity)  │
│   proxy)    │────▶│             │     └──────────────┘
└─────────────┘     └─────────────┘            │
       │                                        │
       └──────────────────┬───────────────────┘
                            ▼
                   ┌─────────────────┐
                   │  Backend API    │
                   │  ASP.NET Core   │
                   │  Modular Mono-  │
                   │  lith           │
                   └────────┬────────┘
                            │
              ┌─────────────┼─────────────┐
              ▼             ▼             ▼
        ┌──────────┐  ┌──────────┐  ┌──────────┐
        │PostgreSQL│  │ Storage  │  │  Audit   │
        └──────────┘  └──────────┘  └──────────┘
```

### Backend structure

```
src/backend/src/
├── HrPortal.Api/              # API host, middleware, DbContext
├── Platform/
│   ├── HrPortal.SharedKernel/ # Base entities, Result, IUnitOfWork
│   ├── HrPortal.Tenancy/      # Multi-tenant resolution & isolation
│   ├── HrPortal.Identity/     # JWT validation (Keycloak)
│   ├── HrPortal.Authorization/# Permission-based authorization ([RequirePermission])
│   ├── HrPortal.Storage/      # IStorageProvider (filesystem)
│   ├── HrPortal.Audit/        # Audit logging
│   ├── HrPortal.Notifications/# Notification abstraction
│   └── HrPortal.Configuration/
└── Modules/
    ├── HrPortal.Employees/    # Reference implementation
    ├── HrPortal.Departments/
    ├── HrPortal.Leave/
    ├── HrPortal.Documents/
    └── HrPortal.Attendance/
```

Each module follows **Clean Architecture**:

```
API → Application → Domain → Infrastructure
```

## Tech stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core 8, EF Core, PostgreSQL |
| Frontend | React 18, TypeScript, Vite, Zustand, Tailwind |
| Auth | Keycloak (OIDC/JWT) |
| Infra | Docker, Docker Compose, Nginx |
| Validation | FluentValidation (BE), Zod (FE) |
| Logging | Serilog (structured) |

## Quick start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)

### 1. Clone and configure

```bash
cp .env.example .env
```

### 2. Start infrastructure

```bash
docker compose up -d postgres keycloak
```

### 3. Run backend

Migrations are committed in the repo and applied automatically on startup.

```bash
cd src/backend
dotnet run --project src/HrPortal.Api
```

API: http://localhost:5000  
Swagger: http://localhost:5000/swagger  
Health: http://localhost:5000/health

**API artifacts:** [OpenAPI spec](docs/openapi/hrportal-v1.json) · [Postman collection](docs/postman/HR-Portal.postman_collection.json)

### 4. Run frontend

```bash
cd src/frontend
npm install
npm run dev
```

Frontend: http://localhost:5173

#### Frontend configuration

Copy `src/frontend/.env.example` to `src/frontend/.env` for local dev, or set variables in the root `.env` for Docker builds.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `VITE_API_BASE_URL` | Production | `/api` | Backend API base URL |
| `VITE_KEYCLOAK_URL` | Yes | — | Keycloak server URL |
| `VITE_KEYCLOAK_REALM` | Yes | — | Keycloak realm (`hrportal`) |
| `VITE_KEYCLOAK_CLIENT_ID` | Yes | — | OIDC client ID (`hrportal-web`) |
| `VITE_TENANCY_MODE` | No | `multi` | `single` or `multi` — controls tenant header behavior |
| `VITE_TENANT_ID` | Multi mode only | `demo` (dev) | Tenant slug sent as `X-Tenant-Id` header |

**Single mode** (`VITE_TENANCY_MODE=single`): for OSS single-organization deployments. The API client omits `X-Tenant-Id`; `VITE_TENANT_ID` is not required. Tenant UI is hidden in the shell.

**Multi mode** (`VITE_TENANCY_MODE=multi`, default): for SaaS multi-tenant deployments. Every API request includes `X-Tenant-Id` from `VITE_TENANT_ID` (required in production builds).

### Full stack with Docker

```bash
docker compose up --build
```

### Database migrations (model changes only)

You only need the EF Core CLI when adding a new migration after changing entities — not for the initial setup.

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

On macOS with Homebrew .NET, add to your shell profile:

```bash
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
export PATH="$PATH:$HOME/.dotnet/tools"
```

```bash
cd src/backend
dotnet ef migrations add <MigrationName> \
  --project src/HrPortal.Api \
  --output-dir Infrastructure/Persistence/Migrations
```

See [Operations guide](docs/OPERATIONS.md#database-migrations) for apply/rollback details.

## Deployment modes

The same codebase supports two deployment profiles via configuration only (ADR-012).

### OSS single-tenant

For self-hosted deployments serving one organization:

| Setting | Value |
|---------|-------|
| `TENANCY__MODE` | `Single` |
| `VITE_TENANCY_MODE` | `single` |

- No `X-Tenant-Id` header required on API requests
- Tenant slug UI hidden in the frontend shell
- `IFeatureGateService` returns Enterprise-equivalent features regardless of persisted plan
- Set both variables in root `.env` and rebuild Docker images when using Compose

### SaaS multi-tenant (default)

For hosted multi-organization deployments:

| Setting | Value |
|---------|-------|
| `TENANCY__MODE` | `Multi` (default) |
| `VITE_TENANCY_MODE` | `multi` (default) |

- Every API request must include `X-Tenant-Id` (header or subdomain resolution)
- Frontend displays current tenant slug in the shell
- Strict tenant isolation via EF global filters and `ApplyTenantScope`
- Demo tenant slug: `demo` (local dev via `VITE_TENANT_ID`)

### Authorization model

Access control is **permission-based**, not legacy ASP.NET role policies. Tenant roles map to permission strings (e.g. `employee.read:tenant`); the frontend loads permissions from `GET /api/v1/me` and gates UI elements with `hasPermission()`. Backend enforcement uses `[RequirePermission]` attributes and the centralized policy engine.

## Multi-tenancy

- **Model**: Shared database with `TenantId` on all business entities
- **Local dev**: Pass tenant via `X-Tenant-Id` header (default: `demo`)
- **Production**: Subdomain resolution (`acme.hrportal.com`)
- **Isolation**: EF Core global query filters — no manual filtering in repositories

## Authentication

Identity is fully delegated to **Keycloak**. The backend only validates JWT tokens and enforces authorization policies.

| Role | Description |
|------|-------------|
| Admin | Platform administrator |
| HR | HR staff |
| Manager | Team manager |
| Employee | Standard employee |

Demo users (after Keycloak import):

| User | Password | Role |
|------|----------|------|
| admin@demo.local | admin123 | Admin |
| hr@demo.local | hr123 | HR |
| employee@demo.local | employee123 | Employee |

## API conventions

- Base path: `/api/v1/`
- Errors: RFC 7807 `ProblemDetails` (no stack traces in production)
- Health: `/health`, `/ready`
- OpenAPI: [`docs/openapi/hrportal-v1.json`](docs/openapi/hrportal-v1.json)
- Postman: [`docs/postman/HR-Portal.postman_collection.json`](docs/postman/HR-Portal.postman_collection.json)

### Example: create employee

```bash
curl -X POST http://localhost:5000/api/v1/employees \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: demo" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "firstName": "Mario",
    "lastName": "Rossi",
    "email": "mario.rossi@demo.local",
    "hireDate": "2024-01-15"
  }'
```

## Operations

Operator runbooks for day-2 operations:

- [Operations guide](docs/OPERATIONS.md) — backup/restore, Keycloak, migrations, logging
- [Deployment checklist](docs/DEPLOYMENT.md) — production rollout and smoke tests

Regenerate API artifacts after endpoint changes:

```bash
./scripts/export-openapi.sh
python3 scripts/generate-postman-collection.py
```

## Development order

1. Platform services
2. Infrastructure (Docker, DB, Nginx)
3. Identity & Tenancy
4. Business modules (Employees first)
5. Frontend
6. Integration & deployment

## Testing

```bash
cd src/backend
dotnet test
```

## License

See [LICENSE](LICENSE).
