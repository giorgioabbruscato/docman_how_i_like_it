# TASK 09 — SECURITY

> Status: **COMPLETED**

Security hardening for production deployment.

## TASK 09.1 — Authentication hardening

- [x] HTTPS enforcement in production
- [x] Keycloak HTTPS configuration
- [x] JWT token expiration and refresh flow
- [x] Secure cookie settings for frontend auth

## TASK 09.2 — Authorization audit

- [x] Review all endpoint policies
- [x] Ensure no endpoint lacks authorization (except health/tenants)
- [x] Tenant isolation integration tests
- [x] Role escalation prevention tests

## TASK 09.3 — Input validation

- [x] All DTOs have FluentValidation validators
- [x] SQL injection prevention (parameterized queries via EF)
- [x] XSS prevention in frontend rendering
- [x] File upload validation (MIME type, size limits)

## TASK 09.4 — Secrets management

- [x] No secrets in source code or docker-compose.yml defaults
- [x] Production secrets via environment variables or vault
- [x] `.env` in `.gitignore`
- [x] Keycloak admin password rotated

## TASK 09.5 — Headers and CORS

- [x] Security headers (X-Content-Type-Options, X-Frame-Options, etc.)
- [x] CORS restricted to known origins in production
- [x] Rate limiting on API endpoints

## TASK 09.6 — Audit trail

- [x] All write operations logged via Audit module
- [x] Audit log includes: user, action, entity, timestamp, tenant
- [x] Audit logs immutable (append-only)

## TASK 09.7 — Dependency scanning

- [x] `dotnet list package --vulnerable`
- [x] `npm audit`
- [x] Container image scanning in CI

## Acceptance criteria

- [x] No hardcoded secrets in codebase
- [x] Tenant isolation verified with integration tests
- [x] All endpoints have appropriate authorization
- [x] Security headers present in production responses

## Next task

→ `10_docs.md` — Documentation finalization
