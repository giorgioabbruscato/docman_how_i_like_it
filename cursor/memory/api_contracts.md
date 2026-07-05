# API CONTRACTS

> Source of truth for all API endpoints. Update this file when adding or modifying endpoints.

## Conventions

- Base path: `/api/v1/`
- Auth: `Authorization: Bearer {jwt_token}`
- Tenant header: `X-Tenant-Id: {tenant_slug}` — behavior depends on deployment mode (see below)
- Errors: RFC 7807 `ProblemDetails`
- Health: `/health`, `/ready` (no auth, no tenant)

### Tenant header by deployment mode (ADR-012)

| Mode | `X-Tenant-Id` | Resolution |
|------|---------------|------------|
| **Multi** (default) | Required on all business endpoints | Header or subdomain; `400` if missing |
| **Single** | Optional | Auto-resolve `DefaultTenantSlug` (default: `demo`) when absent |

Configuration: `Tenancy:Mode` (`Single` \| `Multi`), `Tenancy:DefaultTenantSlug`  
Frontend: `VITE_TENANCY_MODE` (task 32)

## Authorization policies

### Permission-based authorization (ADR-012) — IMPLEMENTED (Task 22)

All V1 business endpoints use `[RequirePermission("{resource}.{action}:{scope}")]` (single permission,
AND with `Authenticated`) or `[RequireAnyPermission(p1, p2, ...)]` (OR semantics) via `IPolicyEngine.Can(ctx, action, resource)`.
`RequireAnyPermission` exists specifically to give parity between tenant-scoped (HR) and team/self-scoped
(Manager/Employee) callers on the same endpoint — e.g. `GET /api/v1/employees` accepts either
`employee.read:tenant` OR `employee.read:team`.

Permissions are resolved per request from the caller's `TenantMembership` → `TenantRole.PermissionsJson`.
Callers without an active membership fall back to `LegacyRoleMapper`, which maps Keycloak realm roles
(Admin/HR/Manager/Employee) to the equivalent `SystemRoleTemplates` permission set — this keeps the
Keycloak-role-only demo users working without requiring an explicit membership row.

### Legacy role policies (DEPRECATED — Task 23, `[Obsolete]`)

`Policies.AdminOnly`, `Policies.HrOrAdmin`, `Policies.ManagerOrAbove` are marked `[Obsolete]` and are no
longer registered as ASP.NET Core authorization policies or applied to any endpoint. They are retained
only as constants to avoid breaking references while call sites are cleaned up; new code must use
`[RequirePermission]` / `[RequireAnyPermission]`. `Policies.Authenticated` remains the baseline
"valid JWT" policy applied at the controller level.

---

## Tenants

### GET /api/v1/tenants

**Auth:** None  
**Response:** `200 OK`

```json
[
  { "id": "uuid", "name": "Demo Company", "slug": "demo", "isActive": true }
]
```

### POST /api/v1/tenants

**Auth:** None  
**Request:**

```json
{ "name": "Acme Corp", "slug": "acme" }
```

**Response:** `201 Created`

```json
{ "id": "uuid", "name": "Acme Corp", "slug": "acme" }
```

---

## Employees

### GET /api/v1/employees

**Auth:** `employee.read:tenant` OR `employee.read:team`  
**Response:** `200 OK`

```json
[
  {
    "id": "uuid",
    "firstName": "Mario",
    "lastName": "Rossi",
    "email": "mario.rossi@demo.local",
    "jobTitle": "Developer",
    "departmentId": "uuid-or-null",
    "hireDate": "2024-01-15",
    "isActive": true
  }
]
```

### GET /api/v1/employees/{id}

**Auth:** `employee.read:tenant` OR `employee.read:team` OR `employee.read:self`  
**Response:** `200 OK` — single EmployeeDto  
**Errors:** `404` if not found

### POST /api/v1/employees

**Auth:** `employee.create:tenant`  
**Request:**

```json
{
  "firstName": "Mario",
  "lastName": "Rossi",
  "email": "mario.rossi@demo.local",
  "hireDate": "2024-01-15",
  "jobTitle": "Developer",
  "departmentId": "uuid-or-null"
}
```

**Response:** `201 Created` — EmployeeDto  
**Errors:** `403 PLAN_LIMIT_EXCEEDED` if the tenant's plan employee limit is reached (see Platform Admin section)

### PUT /api/v1/employees/{id}

**Auth:** `employee.update:tenant`  
**Request:**

```json
{
  "firstName": "Mario",
  "lastName": "Rossi",
  "email": "mario.rossi@demo.local",
  "jobTitle": "Senior Developer",
  "departmentId": "uuid-or-null"
}
```

**Response:** `200 OK` — EmployeeDto

### DELETE /api/v1/employees/{id}

**Auth:** `employee.delete:tenant`  
**Action:** Soft delete (deactivate)  
**Response:** `204 No Content`

---

## Departments

### GET /api/v1/departments

**Auth:** department.read:tenant  
**Response:** `200 OK`

```json
[
  {
    "id": "uuid",
    "name": "Engineering",
    "code": "ENG",
    "description": "Software development",
    "parentDepartmentId": "uuid-or-null",
    "isActive": true
  }
]
```

### GET /api/v1/departments/{id}

**Auth:** department.read:tenant  
**Response:** `200 OK` — single DepartmentDto

### POST /api/v1/departments

**Auth:** `department.write:tenant`  
**Request:**

```json
{
  "name": "Engineering",
  "code": "ENG",
  "description": "Software development",
  "parentDepartmentId": null
}
```

**Response:** `201 Created` — DepartmentDto

### PUT /api/v1/departments/{id}

**Auth:** `department.write:tenant`  
**Request:** Same as POST  
**Response:** `200 OK` — DepartmentDto

### DELETE /api/v1/departments/{id}

**Auth:** `department.delete:tenant`  
**Action:** Soft delete (deactivate)  
**Response:** `204 No Content`

---

## Leave Requests

### GET /api/v1/leave-requests

**Auth:** `leave.read:tenant` OR `leave.read:team`  
**Response:** `200 OK` — array of LeaveRequestDto

### GET /api/v1/leave-requests/{id}

**Auth:** `leave.read:tenant` OR `leave.read:team` OR `leave.read:self`  
**Response:** `200 OK` — single LeaveRequestDto

### POST /api/v1/leave-requests

**Auth:** `leave.create:self`  
**Request:**

```json
{
  "employeeId": "uuid",
  "startDate": "2025-07-01",
  "endDate": "2025-07-05",
  "type": "Annual",
  "reason": "Summer holiday"
}
```

**Response:** `201 Created` — LeaveRequestDto

### PUT /api/v1/leave-requests/{id}/approve

**Auth:** `leave.approve:team`  
**Response:** `200 OK` — LeaveRequestDto

### PUT /api/v1/leave-requests/{id}/reject

**Auth:** `leave.approve:team`  
**Request:**

```json
{ "reason": "Insufficient coverage" }
```

**Response:** `200 OK` — LeaveRequestDto

### DELETE /api/v1/leave-requests/{id}

**Auth:** `leave.delete:self`  
**Action:** Cancel pending request  
**Response:** `204 No Content`

---

## Attendance

### GET /api/v1/attendance

**Auth:** `attendance.read:tenant` OR `attendance.read:team`  
**Response:** `200 OK` — array of AttendanceRecordDto

### POST /api/v1/attendance/check-in

**Auth:** `attendance.write:self`  
**Request:**

```json
{
  "employeeId": "uuid",
  "date": "2025-07-04",
  "time": "09:00:00"
}
```

**Response:** `200 OK` — AttendanceRecordDto

### POST /api/v1/attendance/check-out

**Auth:** `attendance.write:self`  
**Request:** Same shape as check-in  
**Response:** `200 OK` — AttendanceRecordDto

### GET /api/v1/attendance/reports

**Auth:** `attendance.read:tenant` OR `attendance.read:team`  
**Query:** `from`, `to` (DateOnly)  
**Response:** `200 OK` — AttendanceReportDto

---

## Documents

### GET /api/v1/documents

**Auth:** `document.read:tenant`  
**Response:** `200 OK` — array of DocumentDto

### GET /api/v1/documents/{id}

**Auth:** `document.read:tenant` OR `document.read:self`  
**Response:** `200 OK` — single DocumentDto

### POST /api/v1/documents

**Auth:** `document.upload:self`  
**Content-Type:** `multipart/form-data`  
**Form fields:** `employeeId`, `category`, `file`  
**Response:** `201 Created` — DocumentDto

### GET /api/v1/documents/{id}/download

**Auth:** `document.read:tenant` OR `document.read:self`  
**Response:** `200 OK` — file stream

### DELETE /api/v1/documents/{id}

**Auth:** `document.delete:tenant`  
**Response:** `204 No Content`

---

## Access Control — IMPLEMENTED

Centralized RBAC replacing Keycloak-only role checks. Permissions resolved from `TenantMembership` + `TenantRole`, with `LegacyRoleMapper` fallback when no active membership exists.

### GET /api/v1/me

**Auth:** Authenticated  
**Tenant:** Required (`X-Tenant-Id`)  
**Response:** `200 OK`

```json
{
  "userId": "uuid",
  "email": "mario.rossi@demo.local",
  "tenantId": "uuid",
  "tenantSlug": "demo",
  "employeeId": "uuid-or-null",
  "roleSlugs": ["employee"],
  "permissions": ["employee.read:self", "leave.create:self"],
  "features": ["leave", "attendance"],
  "isPlatformAdmin": false,
  "planFeatures": {
    "maxEmployees": 20,
    "customRoles": false,
    "auditLog": false,
    "advancedReports": false
  }
}
```

`features` is the tenant's enabled **module** list (`Tenant.ModulesJson`, e.g. `"leave"`, `"attendance"`).
`planFeatures` is the effective **plan feature** set (`Tenant.Plan` defaults merged with
`Tenant.FeaturesJson` overrides via `IFeatureGateService.GetEffectiveFeaturesAsync()`) — this is what the
frontend uses to gate plan-limited UI such as the audit log page. Single-tenant (OSS) deployments always
get Enterprise-equivalent `planFeatures` regardless of the persisted plan.

### GET /api/v1/roles

**Auth:** `role.read:tenant`  
**Response:** `200 OK` — array of TenantRoleDto

### POST /api/v1/roles

**Auth:** `role.create:tenant`  
**Request:**

```json
{
  "slug": "team-lead",
  "permissions": ["employee.read:team", "leave.approve:team"]
}
```

**Response:** `201 Created` — TenantRoleDto  
**Errors:** `403 PLAN_LIMIT_EXCEEDED` if the tenant's plan does not include the `customRoles` feature (Free plan)

### PUT /api/v1/roles/{id}

**Auth:** `role.update:tenant`  
**Request:** Same as POST  
**Response:** `200 OK` — TenantRoleDto  
**Errors:** `404` if not found; `409` if system role

### DELETE /api/v1/roles/{id}

**Auth:** `role.delete:tenant`  
**Action:** Soft delete (deactivate)  
**Response:** `204 No Content`  
**Errors:** `409` if system role

### GET /api/v1/memberships

**Auth:** `membership.read:tenant`  
**Response:** `200 OK` — array of TenantMembershipDto

### POST /api/v1/memberships

**Auth:** `membership.create:tenant`  
**Request:**

```json
{
  "userId": "uuid",
  "roleIds": ["uuid"],
  "employeeId": "uuid-or-null",
  "attributes": {}
}
```

**Response:** `201 Created` — TenantMembershipDto

### PUT /api/v1/memberships/{id}

**Auth:** `membership.update:tenant`  
**Request:** Same as POST (partial update)  
**Response:** `200 OK` — TenantMembershipDto

### DELETE /api/v1/memberships/{id}

**Auth:** `membership.delete:tenant`  
**Action:** Soft delete (deactivate)  
**Response:** `204 No Content`

---

## Platform Admin — IMPLEMENTED (Task 24)

SaaS plan/feature gates and cross-tenant platform administration. Platform routes
(`/api/v1/platform/*`) are excluded from tenant resolution (`RequestContextMiddleware`) and instead
require `UserProfile.IsPlatformAdmin = true` for the caller's `UserId`; non-platform-admin authenticated
callers get `403`, anonymous callers get `401`. No `X-Tenant-Id` header is used or required.

`TenantPlan`: `Free` | `Pro` | `Enterprise` (single-tenant/OSS deployments always resolve to
Enterprise-equivalent features regardless of the stored plan). Effective features are plan defaults
merged with tenant-specific overrides (`IFeatureGateService`):

| Plan | maxEmployees | customRoles | auditLog | advancedReports |
|------|-------------|-------------|----------|------------------|
| Free | 20 | false | false | false |
| Pro | 200 | true | true | false |
| Enterprise | unlimited | true | true | true |

Feature gates enforced in application services (no inline plan checks in controllers):
`EmployeeService.CreateAsync` (max employees) and `TenantRoleService.CreateAsync` (`customRoles`) return
`Result.Failure(..., "PLAN_LIMIT_EXCEEDED")`, mapped to `403 Forbidden` by the controllers.

### GET /api/v1/platform/tenants

**Auth:** `tenant.manage:all` (platform admin only)  
**Response:** `200 OK`

```json
[
  {
    "id": "uuid",
    "name": "Acme Corp",
    "slug": "acme",
    "isActive": true,
    "isSuspended": false,
    "plan": "Pro",
    "modules": ["employees", "departments", "leave", "attendance", "documents"],
    "features": { "maxEmployees": 200, "customRoles": true, "auditLog": true, "advancedReports": false }
  }
]
```

### POST /api/v1/platform/tenants/{id}/suspend

**Auth:** `tenant.manage:all`  
**Action:** Blocks all further tenant-scoped access (`404` on business endpoints while suspended)  
**Response:** `200 OK` — PlatformTenantDto  
**Errors:** `404` if tenant not found

### POST /api/v1/platform/tenants/{id}/reactivate

**Auth:** `tenant.manage:all`  
**Response:** `200 OK` — PlatformTenantDto  
**Errors:** `404` if tenant not found

### PUT /api/v1/platform/tenants/{id}/plan

**Auth:** `tenant.manage:all`  
**Request:**

```json
{ "plan": "Pro" }
```

**Response:** `200 OK` — PlatformTenantDto  
**Errors:** `400` if plan is not one of Free/Pro/Enterprise; `404` if tenant not found

### PUT /api/v1/platform/tenants/{id}/features

**Auth:** `tenant.manage:all`  
**Request:** (all fields optional; `null` = inherit plan default)

```json
{ "maxEmployees": 500, "customRoles": true, "auditLog": null, "advancedReports": null }
```

**Response:** `200 OK` — PlatformTenantDto  
**Errors:** `404` if tenant not found

---

## Audit Logs — IMPLEMENTED (Task 25)

Enterprise access-decision and business-mutation audit trail. `AuditLog` rows are immutable
(modify/delete throws — enforced in the DbContext `SaveChanges` interceptor); every permission check
performed by `PermissionAuthorizationHandler` / `PermissionAnyAuthorizationHandler` writes an
`Allow`/`Deny` row immediately (`saveImmediately: true`), independent of the request's own unit-of-work,
so read-only (GET) requests are captured too.

### GET /api/v1/audit-logs

**Auth:** `audit.read:tenant`  
**Tenant:** Required (`X-Tenant-Id`)  
**Feature gate:** Requires the tenant's effective `auditLog` plan feature (Enterprise, or Pro/Free with an
override) — checked in the controller via `IFeatureGateService.IsEnabledAsync("auditLog")` before the
query service runs; Free-plan tenants get `403` even if they hold the permission.  
**Query params:** `from`, `to` (ISO-8601 date-times), `actorUserId` (guid), `action`, `decision`
(`Allow`|`Deny`), `page` (default 1), `pageSize` (default 50)  
**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": "uuid",
      "timestamp": "2026-07-05T12:00:00Z",
      "userId": "uuid",
      "actorEmail": "hr@demo.local",
      "action": "employee.read:tenant",
      "entity": "Employee",
      "entityId": "uuid-or-null",
      "targetId": "uuid-or-null",
      "scope": "Tenant",
      "decision": "Allow",
      "ipAddress": "127.0.0.1",
      "metadata": null
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 50
}
```

**Errors:** `401` anonymous; `403` missing `audit.read:tenant` permission, or feature disabled on the
tenant's plan.

**Frontend:** `src/frontend/src/pages/audit-page.tsx` (filters: action, decision, date range;
paginated table). Nav link in `app-layout.tsx` is visible only when
`hasAnyPermission(permissions, Permission.AuditReadTenant) && planFeatures.auditLog` (from `/api/v1/me`).

---

## Auth flow

```
1. User → Keycloak login (OIDC authorization code + PKCE)
2. Keycloak → JWT access token + refresh token
3. Frontend stores token in Zustand auth store (memory only)
4. Frontend → API with Authorization: Bearer {token}
   + X-Tenant-Id: {slug} (Multi mode) or omitted (Single mode)
5. Backend validates JWT against Keycloak authority
6. RequestContextMiddleware resolves tenant (mode-aware)
7. TenantContextFactory enriches context:
   - TenantMembership + TenantRole permissions
   - Legacy Keycloak roles via LegacyRoleMapper (fallback)
8. [RequirePermission] / [RequireAnyPermission] authorization handlers call IPolicyEngine.Can(ctx, permission, resource)
9. Application service executes with TenantContext (no HTTP/JWT access); IFeatureGateService enforces plan limits
10. Repository applies ApplyTenantScope(ctx) on all queries
```

Platform admin requests (`/api/v1/platform/*`) skip steps 6–7 above; see the Platform Admin section.

## Error response format

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not found",
  "status": 404,
  "detail": "Employee with key '...' was not found.",
  "errorCode": "NOT_FOUND"
}
```
