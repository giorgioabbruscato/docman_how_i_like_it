# TASK 28 — CALENDAR INTEGRATION

> Status: **COMPLETED**

Sync approved leave and absences with Google Calendar and Microsoft 365.

## Goal

Allow tenants to connect external calendar providers and automatically sync approved leave events to employee calendars.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 29 — Approval workflows **COMPLETED**
- `HrPortal.Leave` — approved leave as sync source
- OAuth2 infrastructure (sub-deliverable — `HrPortal.Integrations`)

## Deliverables

### OAuth2 sub-deliverable

- [x] `IOAuthTokenStore` — encrypted storage per tenant/employee/provider
- [x] OAuth authorization URL generation for Google and Microsoft
- [x] Callback endpoint to exchange code for tokens
- [x] Token refresh before expiry
- [x] Configuration: `Integrations:Google:ClientId`, `Integrations:Microsoft:ClientId` (secrets via env)

### Domain entities

- [x] `CalendarConnection`, `ExternalCalendarEvent`, `CalendarSyncLog`

### Interface: `ICalendarSyncProvider`

- [x] Google, Microsoft365, and mock implementations

### Permissions

- [x] `calendar_connect.manage:self`, `calendar_sync.manage:tenant`

### API endpoints

- [x] 7 endpoints on `CalendarIntegrationsController`

### Sync behavior

- [x] On leave approved → create/update external calendar event
- [x] On leave cancelled/rejected → delete external event
- [x] Background job: retry failed syncs with exponential backoff
- [x] Sync log for tenant admins

### Tests

- [x] `CalendarSyncMockTests.cs` — OAuth, approval sync, idempotent re-sync, delete

### Frontend

- [x] `calendarIntegrations.ts`, settings connect/disconnect UI, `/settings/calendar/callback`

## Acceptance criteria

- [x] Employees can connect Google or Microsoft calendar via OAuth
- [x] Approved leave syncs to external calendar
- [x] Cancelled/rejected leave removes external event
- [x] Tokens encrypted at rest; no secrets in code
- [x] Mock provider tests pass in CI
- [x] `dotnet test` green
- [x] `npm run build` green

## Next task

→ `30_admin_dashboard.md` — Platform admin metrics
