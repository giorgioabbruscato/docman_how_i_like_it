# DESIGN PATTERNS

## Mandatory patterns

| Pattern | Usage |
|---------|-------|
| **Repository** | Data access — `I{Entity}Repository` in Application, implementation in Infrastructure |
| **Service Layer** | Business orchestration — `I{Entity}Service` in Application |
| **Result** | Error handling — `Result<T>` instead of exceptions for expected failures |
| **Factory Method** | Domain entity creation — `{Entity}.Create(...)` static methods |
| **Dependency Injection** | Everywhere — register via `{Module}ServiceCollectionExtensions` |
| **DTO separation** | Request/response records separate from domain entities |
| **Options pattern** | Configuration — `IOptions<{Feature}Options>` |
| **Unit of Work** | Transaction boundary — `IUnitOfWork.SaveChangesAsync()` |

## Optional patterns

- **CQRS** — for complex modules with divergent read/write models
- **Specification** — for complex query logic in repositories
- **Domain Events** — for cross-module side effects (future)

## Module structure template

```
HrPortal.{Module}/
├── Domain/
│   └── {Entity}.cs
├── Application/
│   ├── Dtos/
│   ├── Validators/
│   ├── I{Entity}Repository.cs
│   ├── I{Entity}Service.cs
│   └── {Entity}Service.cs
├── Infrastructure/
│   └── Persistence/
│       ├── {Entity}Configuration.cs
│       └── {Entity}Repository.cs
└── {Module}ServiceCollectionExtensions.cs
```

## Controller pattern

Controllers must:
1. Accept DTOs via `[FromBody]`
2. Call application service
3. Map `Result<T>` to HTTP status codes
4. Contain zero business logic

## Avoid

- Active Record pattern (anemic entities with EF attributes)
- Fat controllers
- God services spanning multiple domains
- Shared mutable state
- Direct `DbContext` usage outside repositories

## Reference implementation

`HrPortal.Employees` is the canonical module. Copy its structure for new modules.
