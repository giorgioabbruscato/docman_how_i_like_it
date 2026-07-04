# AGENT RULES (GLOBAL)

You are a deterministic code generation agent working on the **HR Portal** modular monolith.

## Mandatory rules

- Always follow Clean Architecture
- Never mix domain and infrastructure
- Controllers must be thin — delegate to application services
- Business logic must live in Application/Domain layers only
- All dependencies must be injected via interfaces
- No hardcoded configuration — use `IOptions<T>` or environment variables
- All external systems must be abstracted behind interfaces
- Multi-tenancy is mandatory — every business entity implements `ITenantEntity`
- Cross-module communication goes through public application services, never direct DB access

## Project layout

```
src/backend/src/
├── HrPortal.Api/           # Transport layer, middleware, DbContext host
├── Platform/               # Cross-cutting platform services (no domain deps)
└── Modules/                # Independent domain modules

src/frontend/src/
├── api/                    # Typed API clients (no direct axios in pages)
├── components/             # Reusable UI
├── pages/                  # Route-level components
└── stores/                 # Global state (auth, etc.)
```

## Reference docs

- Architecture: `/cursor/core/03_architecture.md`
- Patterns: `/cursor/core/04_patterns.md`
- Domain model: `/cursor/memory/domain_model.md`
- API contracts: `/cursor/memory/api_contracts.md`
- ADRs: `/cursor/memory/architecture_decisions.md`

## When in doubt

Prefer simplicity over complexity. Match existing module conventions (see `HrPortal.Employees` as reference).
