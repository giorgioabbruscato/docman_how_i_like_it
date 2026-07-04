# TASK 04 — AUTH BACKEND

> Status: **COMPLETED**

JWT authentication and policy-based authorization.

## TASK 04.1 — JWT validation

**Goal:** Validate Keycloak-issued tokens on every request.

**Deliverables:**
- [x] `HrPortal.Identity` module
- [x] JWT Bearer authentication configured in `Program.cs`
- [x] Authority and audience from configuration
- [x] Role claims extracted from Keycloak token

## TASK 04.2 — Authorization policies

**Goal:** Role-based access control via ASP.NET policies.

**Deliverables:**
- [x] `Policies` class with named policies
- [x] `AdminOnly` — requires `admin` role
- [x] `HrOrAdmin` — requires `hr` or `admin` role
- [x] `ManagerOrAbove` — requires `manager`, `hr`, or `admin` role
- [x] `Authenticated` — any valid JWT

## TASK 04.3 — Controller authorization

**Goal:** Apply policies per endpoint.

**Current mapping:**

| Endpoint | Policy |
|----------|--------|
| `GET /employees` | ManagerOrAbove |
| `GET /employees/{id}` | Authenticated |
| `POST /employees` | HrOrAdmin |
| `PUT /employees/{id}` | HrOrAdmin |
| `DELETE /employees/{id}` | HrOrAdmin |
| `GET /departments` | Authenticated |
| `POST /departments` | HrOrAdmin |
| `GET /tenants` | AllowAnonymous |
| `POST /tenants` | AllowAnonymous |

## TASK 04.4 — User context

**Goal:** Extract user identity from JWT claims in services.

**Deliverables:**
- [x] `UserContext` with user ID, email, roles
- [x] Available via DI in application services
- [x] Used for audit fields (`CreatedBy`, `UpdatedBy`)

## Acceptance criteria

- [x] Unauthenticated requests to protected endpoints return 401
- [x] Authenticated but unauthorized requests return 403
- [x] Role claims correctly mapped from Keycloak realm roles
- [x] Integration tests for each policy combination

## Next task

→ `05_modules.md` — Cross-module integration patterns
