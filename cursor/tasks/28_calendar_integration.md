# TASK 28 — CALENDAR INTEGRATION

> Status: **PENDING**

Sync approved leave and absences with Google Calendar and Microsoft 365.

## Goal

Allow tenants to connect external calendar providers and automatically sync approved leave events to employee calendars.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 29 — Approval workflows (recommended sequence)
- `HrPortal.Leave` — approved leave as sync source
- OAuth2 infrastructure (sub-deliverable — not yet a dedicated module)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | No hardcoded secrets |
| TDD | `cursor/core/01_tdd.md` | Tests required |
| Patterns | `cursor/core/04_patterns.md` | Platform service template |
| Architecture | `cursor/core/03_architecture.md` | Platform vs module |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Domain model | `cursor/memory/domain_model.md` | Integration entities |
| API contracts | `cursor/memory/api_contracts.md` | Integration endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | Add Integrations |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- New `HrPortal.Integrations` platform service
- `ICalendarSyncProvider` interface with Google Calendar and Microsoft 365 implementations
- OAuth2 tokens stored encrypted per tenant + employee connection
- Secrets from configuration / environment — never in source code
- Sync triggered on leave approval (event-driven or background job)
- Idempotent sync: external event ID stored to avoid duplicates
- Disconnect/revoke flow clears tokens
- Mock providers for integration tests — no live API calls in CI

### Memory — source of truth (`cursor/memory/`)

- Add `CalendarConnection`, `ExternalCalendarEvent` to `domain_model.md`
- Document OAuth and sync endpoints in `api_contracts.md`
- Update `module_dependencies.md`
- Add ADR for OAuth token storage

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Leave module and Task 22 are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update memory files
3. Mark task status **COMPLETED**

## Deliverables

### OAuth2 sub-deliverable

- [ ] `IOAuthTokenStore` — encrypted storage per tenant/employee/provider
- [ ] OAuth authorization URL generation for Google and Microsoft
- [ ] Callback endpoint to exchange code for tokens
- [ ] Token refresh before expiry
- [ ] Configuration: `Integrations:Google:ClientId`, `Integrations:Microsoft:ClientId` (secrets via env)

### Domain entities

#### `CalendarConnection`

| Field | Type | Notes |
|-------|------|-------|
| EmployeeId | Guid | Connected employee |
| Provider | CalendarProvider | Google, Microsoft365 |
| ExternalCalendarId | string? | Target calendar ID |
| AccessTokenEncrypted | string | Encrypted |
| RefreshTokenEncrypted | string? | Encrypted |
| TokenExpiresAt | DateTime? | UTC |
| ConnectedAt | DateTime | UTC |
| IsActive | bool | |

#### `ExternalCalendarEvent`

| Field | Type | Notes |
|-------|------|-------|
| LeaveRequestId | Guid | Source leave |
| Provider | CalendarProvider | |
| ExternalEventId | string | Provider event ID |
| LastSyncedAt | DateTime | UTC |

### Interface: `ICalendarSyncProvider`

- [ ] `GetAuthorizationUrl(state, redirectUri)` 
- [ ] `ExchangeCodeAsync(code, redirectUri)` → tokens
- [ ] `CreateOrUpdateEventAsync(connection, leaveRequest)` → external event ID
- [ ] `DeleteEventAsync(connection, externalEventId)`
- [ ] Implementations: `GoogleCalendarSyncProvider`, `Microsoft365CalendarSyncProvider`

### Permissions

| Constant | Value |
|----------|-------|
| `CalendarConnectSelf` | `calendar_connect.manage:self` |
| `CalendarSyncManageTenant` | `calendar_sync.manage:tenant` |

### API endpoints

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/integrations/calendar/providers` | authenticated |
| GET | `/api/v1/integrations/calendar/connect/{provider}` | `calendar_connect.manage:self` |
| GET | `/api/v1/integrations/calendar/callback` | public (OAuth callback) |
| GET | `/api/v1/integrations/calendar/connections` | `calendar_connect.manage:self` |
| DELETE | `/api/v1/integrations/calendar/connections/{id}` | `calendar_connect.manage:self` |
| POST | `/api/v1/integrations/calendar/sync/{leaveRequestId}` | `calendar_sync.manage:tenant` |
| GET | `/api/v1/integrations/calendar/sync-log` | `calendar_sync.manage:tenant` |

### Sync behavior

- [ ] On leave approved → create/update external calendar event (all-day or timed based on leave type)
- [ ] On leave cancelled/rejected → delete external event if exists
- [ ] Background job: retry failed syncs with exponential backoff
- [ ] Sync log for tenant admins (success/failure per leave request)

### Tests

- [ ] Unit: mock provider create/update/delete
- [ ] Integration: OAuth callback stores encrypted tokens (mock HTTP)
- [ ] Integration: leave approval triggers sync (mock provider)
- [ ] Integration: idempotent re-sync does not duplicate events

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Platform/HrPortal.Integrations/**` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/CalendarIntegrationsController.cs` | Create |
| `src/backend/src/Modules/HrPortal.Leave/**` | Hook on approval |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `src/backend/tests/HrPortal.IntegrationTests/CalendarSyncMockTests.cs` | Create |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/api_contracts.md` | Update |
| `cursor/memory/module_dependencies.md` | Update |
| `cursor/memory/architecture_decisions.md` | Add OAuth ADR |

## Acceptance criteria

- [ ] Employees can connect Google or Microsoft calendar via OAuth
- [ ] Approved leave syncs to external calendar
- [ ] Cancelled leave removes external event
- [ ] Tokens encrypted at rest; no secrets in code
- [ ] Mock provider tests pass in CI
- [ ] `dotnet test` green

## Next task

→ `30_admin_dashboard.md` — Platform admin metrics
