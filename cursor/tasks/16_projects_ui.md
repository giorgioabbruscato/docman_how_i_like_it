# TASK 16 — PROJECTS UI

> Status: **PENDING**

Create Projects frontend pages with list, details, members, CRUD, filters, and search.

## Goal

Build responsive Projects UI using Tailwind, React Query, and Zustand following existing Employees/Departments page patterns.

## Depends on

- Task 02 — Project members (backend APIs complete)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | API client pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| API contracts | `cursor/memory/api_contracts.md` | Projects endpoints |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + frontend eval)

- All HTTP calls via typed API client in `src/frontend/src/api/projects.ts`
- No direct axios in page components
- TypeScript interfaces in `src/frontend/src/types/project.ts`
- React Query for server state (`useQuery`, `useMutation`)
- Zustand only for UI-local state (filters panel open, selected tab)
- Permission-gated actions via `hasPermission()` from auth store
- Responsive layout with Tailwind
- Loading and error states on all async operations
- Form validation before submission

### Memory — source of truth (`cursor/memory/`)

- Reference `api_contracts.md` for endpoint shapes

### Quality gates (`cursor/evals/`)

- `02_frontend_quality_checks.md` — architecture, UI, build checklist

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 02 backend is **COMPLETED**
3. Study existing Employees page as reference

### Before completing

1. Run `npm run build` in `src/frontend`
2. Verify against `02_frontend_quality_checks.md`
3. Mark task status **COMPLETED**

## Deliverables

### API client

- [ ] `src/frontend/src/api/projects.ts` — getProjects, getProject, createProject, updateProject, deleteProject
- [ ] Member methods: getProjectMembers, addProjectMember, removeProjectMember
- [ ] Query params for pagination, search, customer, status, isArchived

### Types

- [ ] `src/frontend/src/types/project.ts` — ProjectDto, CreateProjectRequest, ProjectMemberDto, ProjectStatus enum

### Pages

- [ ] `/projects` — Project list with table/cards, search, filters, pagination
- [ ] `/projects/new` — Create project form
- [ ] `/projects/:id` — Project details with tabs (info, members)
- [ ] `/projects/:id/edit` — Edit project form

### Components

- [ ] `ProjectList` — table with sortable columns
- [ ] `ProjectForm` — shared create/edit form
- [ ] `ProjectMemberList` — members table with add/remove
- [ ] `ProjectFilters` — status, customer, archived toggles
- [ ] `ProjectStatusBadge` — status indicator

### Navigation

- [ ] Add "Projects" to sidebar nav (gated by `project.read:tenant`)
- [ ] Register routes in app router

### React Query hooks

- [ ] `useProjects(filters)` — paginated list
- [ ] `useProject(id)` — single project
- [ ] `useProjectMembers(projectId)`
- [ ] Mutations with cache invalidation on create/update/delete

### Tests (when suite exists)

- [ ] Project list renders with mocked API
- [ ] Create form validation

## Files to touch

| File | Action |
|------|--------|
| `src/frontend/src/api/projects.ts` | Create |
| `src/frontend/src/types/project.ts` | Create |
| `src/frontend/src/pages/projects/*` | Create |
| `src/frontend/src/components/projects/*` | Create |
| `src/frontend/src/App.tsx` or router config | Add routes |
| `src/frontend/src/components/layout/app-layout.tsx` | Add nav item |

## Acceptance criteria

- [ ] Project list with search and filters works
- [ ] Create, edit, delete project flows work
- [ ] Members management on detail page
- [ ] Permission-gated buttons hidden for unauthorized users
- [ ] Responsive on mobile
- [ ] `npm run build` passes with zero TS errors

## Next task

→ `17_time_tracking_ui.md` — Time Tracking frontend
