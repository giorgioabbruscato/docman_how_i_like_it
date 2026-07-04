# TASK 09 — SECURITY

> Status: **PLANNED**

Security hardening for production deployment.

## TASK 09.1 — Authentication hardening

- [ ] HTTPS enforcement in production
- [ ] Keycloak HTTPS configuration
- [ ] JWT token expiration and refresh flow
- [ ] Secure cookie settings for frontend auth

## TASK 09.2 — Authorization audit

- [ ] Review all endpoint policies
- [ ] Ensure no endpoint lacks authorization (except health/tenants)
- [ ] Tenant isolation integration tests
- [ ] Role escalation prevention tests

## TASK 09.3 — Input validation

- [ ] All DTOs have FluentValidation validators
- [ ] SQL injection prevention (parameterized queries via EF)
- [ ] XSS prevention in frontend rendering
- [ ] File upload validation (MIME type, size limits)

## TASK 09.4 — Secrets management

- [ ] No secrets in source code or docker-compose.yml defaults
- [ ] Production secrets via environment variables or vault
- [ ] `.env` in `.gitignore`
- [ ] Keycloak admin password rotated

## TASK 09.5 — Headers and CORS

- [ ] Security headers (X-Content-Type-Options, X-Frame-Options, etc.)
- [ ] CORS restricted to known origins in production
- [ ] Rate limiting on API endpoints

## TASK 09.6 — Audit trail

- [ ] All write operations logged via Audit module
- [ ] Audit log includes: user, action, entity, timestamp, tenant
- [ ] Audit logs immutable (append-only)

## TASK 09.7 — Dependency scanning

- [ ] `dotnet list package --vulnerable`
- [ ] `npm audit`
- [ ] Container image scanning in CI

## Acceptance criteria

- [ ] No hardcoded secrets in codebase
- [ ] Tenant isolation verified with integration tests
- [ ] All endpoints have appropriate authorization
- [ ] Security headers present in production responses

## Next task

→ `10_docs.md` — Documentation finalization
