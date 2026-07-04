# TEST DRIVEN DEVELOPMENT RULES

## Required behavior

- Write tests before implementation when possible
- Use AAA pattern (Arrange, Act, Assert)
- Mock all external dependencies in unit tests
- Every API endpoint must have integration tests
- Every domain rule must have unit tests

## Backend

| Layer | Tool | Location |
|-------|------|----------|
| Unit tests | xUnit + Moq/NSubstitute | `src/backend/tests/HrPortal.UnitTests/` |
| Integration tests | xUnit + WebApplicationFactory | `src/backend/tests/HrPortal.IntegrationTests/` |

### Integration test conventions

- Use `WebApplicationFactory<Program>` with test database or in-memory provider
- Always set `X-Tenant-Id` header in requests
- Test auth policies with valid/invalid JWT claims
- Assert RFC 7807 `ProblemDetails` on error responses

### Unit test conventions

- Test domain entity factory methods and state transitions
- Test validators independently
- Test service layer with mocked repositories and `IUnitOfWork`

## Frontend

- Use **React Testing Library**
- Test UI behavior, not implementation details
- Mock API clients, not axios directly
- Test route protection and auth state transitions

## Definition of Done

**No feature is complete without tests.**

Before marking a task done, run:

```bash
cd src/backend && dotnet test
cd src/frontend && npm test   # when test suite exists
```

## Test naming

```
MethodName_Scenario_ExpectedResult
```

Example: `CreateAsync_DuplicateEmail_ReturnsConflict`
