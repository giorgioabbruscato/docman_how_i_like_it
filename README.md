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
│   ├── HrPortal.Authorization/# Policy-based roles
│   ├── HrPortal.Storage/      # IStorageProvider (filesystem)
│   ├── HrPortal.Audit/        # Audit logging
│   ├── HrPortal.Notifications/# Notification abstraction
│   └── HrPortal.Configuration/
└── Modules/
    ├── HrPortal.Employees/    # Reference implementation
    ├── HrPortal.Departments/  # Planned
    ├── HrPortal.Leave/        # Planned
    └── ...
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

```bash
cd src/backend

# Create initial migration (first time only)
dotnet ef migrations add InitialCreate \
  --project src/HrPortal.Api \
  --output-dir Infrastructure/Persistence/Migrations

dotnet run --project src/HrPortal.Api
```

API: http://localhost:5000  
Swagger: http://localhost:5000/swagger  
Health: http://localhost:5000/health

### 4. Run frontend

```bash
cd src/frontend
npm install
npm run dev
```

Frontend: http://localhost:5173

### Full stack with Docker

```bash
docker compose up --build
```

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

## API conventions

- Base path: `/api/v1/`
- Errors: RFC 7807 `ProblemDetails` (no stack traces in production)
- Health: `/health`, `/ready`

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
