# API CONTRACTS

> Source of truth for all API endpoints. Update this file when adding or modifying endpoints.

## Conventions

- Base path: `/api/v1/`
- Auth: `Authorization: Bearer {jwt_token}`
- Tenant: `X-Tenant-Id: {tenant_slug}` (required on all business endpoints)
- Errors: RFC 7807 `ProblemDetails`
- Health: `/health`, `/ready` (no auth, no tenant)

## Authorization policies

| Policy | Roles |
|--------|-------|
| `Authenticated` | Any valid JWT |
| `ManagerOrAbove` | manager, hr, admin |
| `HrOrAdmin` | hr, admin |
| `AdminOnly` | admin |

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

**Auth:** ManagerOrAbove  
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

**Auth:** Authenticated  
**Response:** `200 OK` — single EmployeeDto  
**Errors:** `404` if not found

### POST /api/v1/employees

**Auth:** HrOrAdmin  
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

### PUT /api/v1/employees/{id}

**Auth:** HrOrAdmin  
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

**Auth:** HrOrAdmin  
**Action:** Soft delete (deactivate)  
**Response:** `204 No Content`

---

## Departments

### GET /api/v1/departments

**Auth:** Authenticated  
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

**Auth:** Authenticated  
**Response:** `200 OK` — single DepartmentDto

### POST /api/v1/departments

**Auth:** HrOrAdmin  
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

**Auth:** HrOrAdmin  
**Request:** Same as POST  
**Response:** `200 OK` — DepartmentDto

### DELETE /api/v1/departments/{id}

**Auth:** HrOrAdmin  
**Action:** Soft delete (deactivate)  
**Response:** `204 No Content`

---

## Planned endpoints

### Leave Requests (not yet implemented)

```
GET    /api/v1/leave-requests
GET    /api/v1/leave-requests/{id}
POST   /api/v1/leave-requests
PUT    /api/v1/leave-requests/{id}/approve
PUT    /api/v1/leave-requests/{id}/reject
DELETE /api/v1/leave-requests/{id}
```

### Attendance (not yet implemented)

```
GET    /api/v1/attendance
POST   /api/v1/attendance/check-in
POST   /api/v1/attendance/check-out
GET    /api/v1/attendance/reports
```

### Documents (not yet implemented)

```
GET    /api/v1/documents
GET    /api/v1/documents/{id}
POST   /api/v1/documents          (multipart/form-data)
GET    /api/v1/documents/{id}/download
DELETE /api/v1/documents/{id}
```

---

## Auth flow

```
1. User → Keycloak login (OIDC authorization code + PKCE)
2. Keycloak → JWT access token + refresh token
3. Frontend stores token in Zustand auth store
4. Frontend → API with Authorization: Bearer {token} + X-Tenant-Id: demo
5. Backend validates JWT against Keycloak authority
6. Backend extracts roles from JWT claims
7. Backend applies authorization policy
8. Backend resolves tenant from X-Tenant-Id header
9. Backend executes request with tenant-scoped data
```

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
