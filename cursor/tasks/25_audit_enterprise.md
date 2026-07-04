# TASK 25 — AUDIT ENTERPRISE

> Status: **PENDING**

Extend the audit module for enterprise-grade access decision logging and audit log querying.

## Goal

Provide immutable audit trail with access decision logging for compliance. Audit log UI gated by Enterprise feature.

## Depends on

- Task 21 — Authorization handler (LogAccessDecisionAsync)
- Task 24 — Feature plans (auditLog feature gate)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` |  |
| Guardrails | `cursor/core/02_guardrails.md` | No secrets hardcoded |
| Architecture | `cursor/core/03_architecture.md` | Platform services |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` |  |
| Domain model | `cursor/memory/domain_model.md` | TenantPlan, TenantFeatures |
| API contracts | `cursor/memory/api_contracts.md` | Platform admin endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Platform services |
| Guardrails | `cursor/core/02_guardrails.md` | Audit logs immutable |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Audit page |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | Page checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Feature gates via IFeatureGateService — no inline plan checks in controllers
- Platform admin routes require IsPlatformAdmin + tenant.manage:all permission
- Services return Result<T> for feature limit violations
- Audit logs immutable — modify/delete throws (existing guardrail)
- LogAccessDecisionAsync on every permission check
- Audit API gated by Enterprise feature + audit.read:tenant permission

### Memory — source of truth (`cursor/memory/`)

- Update `domain_model.md`: TenantPlan, TenantFeatures
- Update `api_contracts.md`: /api/v1/platform/tenants endpoints

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — platform services checklist

### Agent prompts (`cursor/prompts/`)

- `01_backend_agent_prompt.md`
- `00_master_prompt.md`

### Before starting
1. Read this task file and listed `cursor/core/` + `cursor/memory/` references
2. Check `/cursor/evals/` quality gates for this task type
3. Follow `/cursor/prompts/00_master_prompt.md` workflow

- Backend: `01_backend_agent_prompt.md`; Frontend: `02_frontend_agent_prompt.md`

### Before completing
1. Run quality commands listed in Acceptance criteria
2. Verify against applicable `/cursor/evals/` checklist
3. Update `/cursor/memory/` if domain model or API contracts changed
4. Mark task status **COMPLETED** in this file

## Deliverables

### Extended AuditLog entity

- [ ] Add fields: `TargetId`, `Scope`, `IpAddress`, `ActorEmail`, `Decision` (Allow/Deny)
- [ ] Migration for new columns
- [ ] Keep immutability enforcement in DbContext

### Audit service

- [ ] `LogAccessDecisionAsync(ctx, permission, resource, allowed)` — called from policy handler
- [ ] Existing `LogAsync` for business mutations unchanged

### Query API

- [ ] `IAuditQueryService` with filtered pagination
- [ ] `GET /api/v1/audit-logs` — gated by:
  - Enterprise `auditLog` feature enabled
  - `audit.read:tenant` permission
- [ ] Filters: date range, actor, action, decision

### Frontend

- [ ] `src/frontend/src/api/audit-logs.ts` — API client
- [ ] `src/frontend/src/pages/audit-page.tsx` — paginated audit log table
- [ ] Nav link visible only with `audit.read:tenant` + feature enabled

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Audit/Domain/AuditLog.cs` | Extend |
| `HrPortal.Audit/Infrastructure/AuditService.cs` | LogAccessDecision |
| `HrPortal.AccessControl/Application/Services/AuditQueryService.cs` | Create |
| `HrPortal.Api/Controllers/V1/AuditLogsController.cs` | Create |
| `frontend/src/pages/audit-page.tsx` | Create |
| `frontend/src/app.tsx` | Route |

## Acceptance criteria

- [ ] Every permission check logged with allow/deny
- [ ] Audit logs immutable (modify/delete throws)
- [ ] Free plan: audit API returns 403 or feature disabled response
- [ ] Enterprise plan: audit page loads and paginates
- [ ] Integration test for audit log query with permission gate

## Next task

→ `26_service_context_unification.md` — Service context unification
