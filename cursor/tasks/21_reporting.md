# TASK 21 — REPORTING

> Status: **PENDING**

Implement report generation for attendance, projects, worked hours, employees, and departments.

## Goal

Generate downloadable reports in PDF, Excel, and CSV formats with filters, reusing export infrastructure from Task 08.

## Depends on

- Task 08 — Export worked hours
- Task 13 — Analytics module (KPI data sources)
- Task 12 — Attendance dashboard

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Interface abstraction |
| Guardrails | `cursor/core/02_guardrails.md` | Permission scoping |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Report endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Create `HrPortal.Reporting` module OR sub-namespace in Analytics — prefer dedicated module for separation
- Reports behind `IReportGenerator` interface per report type
- Reuse export libraries from Task 08 (ClosedXML, CsvHelper, QuestPDF)
- Permission: `report.generate:team` or `report.generate:tenant`
- Employee role: can generate self-scoped reports only
- No business logic in controllers

### Memory — source of truth (`cursor/memory/`)

- Document report endpoints in `api_contracts.md`
- Add Reporting to `module_dependencies.md`

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 08 and 13 are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update memory files
3. Mark task status **COMPLETED**

## Deliverables

### Module scaffold

- [ ] Create `HrPortal.Reporting` under `src/backend/src/Modules/`
- [ ] Register in `Program.cs`

### Permissions

| Constant | Value |
|----------|-------|
| `ReportGenerateTeam` | `report.generate:team` |
| `ReportGenerateTenant` | `report.generate:tenant` |
| `ReportGenerateSelf` | `report.generate:self` |

### API endpoint

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/reports/{type}` | `report.generate:*` | Generate report file |

### Report types

| Type | Data source | Columns |
|------|-------------|---------|
| `attendance` | AttendanceSession | Employee, date, check-in, check-out, hours, status |
| `projects` | Project + members | Name, customer, status, budget, spent hours, members count |
| `worked-hours` | TimeEntry | Reuse Task 08 export logic |
| `employees` | Employee + department | Name, email, department, hire date, status |
| `departments` | Department + headcount | Name, code, employee count, parent |

### Query parameters

| Param | Type | Description |
|-------|------|-------------|
| `format` | string | `csv`, `xlsx`, `pdf` |
| `fromDate` | DateOnly? | Range start |
| `toDate` | DateOnly? | Range end |
| `departmentId` | Guid? | Filter |
| `projectId` | Guid? | Filter |
| `employeeId` | Guid? | Filter |

### Report generators

- [ ] `IReportGenerator` base interface
- [ ] `AttendanceReportGenerator`
- [ ] `ProjectsReportGenerator`
- [ ] `WorkedHoursReportGenerator` (delegates to Task 08 export service)
- [ ] `EmployeesReportGenerator`
- [ ] `DepartmentsReportGenerator`
- [ ] `ReportGeneratorFactory` resolves by type string

### Tests

- [ ] Unit: each generator produces valid output bytes
- [ ] Integration: endpoint returns correct content-type per format
- [ ] Integration: self scope limits employee report to own data
- [ ] Integration: 403 without permission

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Reporting/**` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/ReportsController.cs` | Create |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `cursor/memory/api_contracts.md` | Update |
| `cursor/memory/module_dependencies.md` | Update |
| `src/backend/tests/HrPortal.IntegrationTests/ReportsEndpointTests.cs` | Create |

## Acceptance criteria

- [ ] All 5 report types generate CSV, XLSX, PDF
- [ ] Filters applied correctly
- [ ] Permission scoping enforced
- [ ] Reuses Task 08 export for worked-hours
- [ ] `dotnet test` green

## Next task

→ `22_documentation_sync.md` — Final documentation audit
