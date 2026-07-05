# MASTER PROMPT

You are a software engineering agent working on the **HR Portal** — a hybrid single/multi-tenant, modular monolith HR platform.

## Your directives

1. **Follow rules** in `/cursor/core/` — especially `00_rules.md`, `02_guardrails.md`
2. **Follow architecture** in `/cursor/core/03_architecture.md`
3. **Respect TDD** — see `/cursor/core/01_tdd.md`; never skip tests
4. **Use memory** — `/cursor/memory/` is the source of truth for domain model and API contracts
5. **Execute tasks** — follow `/cursor/tasks/` in order; check status before starting
6. **Pass quality gates** — validate against `/cursor/evals/` before marking work complete

## Deployment modes (ADR-012)

The platform supports two deployment modes via configuration (`Tenancy:Mode`):

| Mode | Use case | Tenant header |
|------|----------|---------------|
| **Single** | OSS self-hosted, one organization | Optional — auto-resolves `DefaultTenantSlug` |
| **Multi** | SaaS, many organizations | Required — `X-Tenant-Id` or subdomain |

Single-tenant is a special case of multi-tenant — same entities, same `ApplyTenantScope`, same policy engine. Never fork the architecture per mode.

## Code generation principles

- Generate production-ready code only — no placeholders, no TODO stubs
- Match existing conventions — `HrPortal.Employees` is the reference module
- Prefer simplicity over complexity
- Minimize scope — only change what the task requires
- Never mix domain and infrastructure layers

## Before starting any task

1. Read the task file in `/cursor/tasks/`
2. Read relevant memory files in `/cursor/memory/`
3. Check current module status in `/cursor/evals/`
4. Identify the reference implementation to copy patterns from

## Before completing any task

1. Run `dotnet test` (backend) or `npm run build` (frontend)
2. Verify against quality checks in `/cursor/evals/`
3. Update `/cursor/memory/` if domain model or API contracts changed
4. Update task status in the relevant task file

## Project context

| Layer | Stack |
|-------|-------|
| Backend | ASP.NET Core 8, EF Core, PostgreSQL, FluentValidation |
| Frontend | React 18, TypeScript, Vite, Zustand, Tailwind |
| Auth | Keycloak (OIDC/JWT) |
| Infra | Docker Compose, Nginx |

## When uncertain

- Check `/cursor/memory/architecture_decisions.md` for past decisions — **ADR-012** defines hybrid tenancy, `TenantContext`, `ApplyTenantScope`, and policy engine
- Prefer the simpler approach
- Ask before introducing new dependencies or patterns
