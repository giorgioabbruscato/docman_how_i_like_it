import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { fetchDepartments } from '@/api/departments';
import { fetchEmployees } from '@/api/employees';
import { getProjects } from '@/api/projects';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import {
  getThisMonthDateRange,
  getThisWeekDateRange,
  getTodayDateRange,
} from '@/lib/utils';
import type { AnalyticsFilters } from '@/types/analytics';
import type { Department } from '@/types/department';
import type { Employee } from '@/types/employee';
import type { ProjectDto } from '@/types/project';

export type DatePreset = 'today' | 'week' | 'month' | 'custom';

function parseFilters(searchParams: URLSearchParams): AnalyticsFilters {
  return {
    departmentId: searchParams.get('departmentId') ?? undefined,
    projectId: searchParams.get('projectId') ?? undefined,
    employeeId: searchParams.get('employeeId') ?? undefined,
    fromDate: searchParams.get('fromDate') ?? undefined,
    toDate: searchParams.get('toDate') ?? undefined,
  };
}

function filtersToSearchParams(filters: AnalyticsFilters): URLSearchParams {
  const params = new URLSearchParams();
  if (filters.departmentId) params.set('departmentId', filters.departmentId);
  if (filters.projectId) params.set('projectId', filters.projectId);
  if (filters.employeeId) params.set('employeeId', filters.employeeId);
  if (filters.fromDate) params.set('fromDate', filters.fromDate);
  if (filters.toDate) params.set('toDate', filters.toDate);
  return params;
}

export function useAnalyticsFilters() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters = useMemo(() => parseFilters(searchParams), [searchParams]);

  const setFilters = (updates: Partial<AnalyticsFilters>) => {
    const next = { ...parseFilters(searchParams), ...updates };
    setSearchParams(filtersToSearchParams(next), { replace: true });
  };

  const setDatePreset = (preset: DatePreset) => {
    if (preset === 'custom') return;

    const range =
      preset === 'today'
        ? getTodayDateRange()
        : preset === 'week'
          ? getThisWeekDateRange()
          : getThisMonthDateRange();

    setFilters(range);
  };

  useEffect(() => {
    if (!searchParams.get('fromDate') && !searchParams.get('toDate')) {
      const range = getThisMonthDateRange();
      setSearchParams(
        filtersToSearchParams({ ...parseFilters(searchParams), ...range }),
        { replace: true },
      );
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return { filters, setFilters, setDatePreset };
}

export function AnalyticsFilters() {
  const { filters, setFilters, setDatePreset } = useAnalyticsFilters();
  const [departments, setDepartments] = useState<Department[]>([]);
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [preset, setPreset] = useState<DatePreset>('month');

  useEffect(() => {
    void fetchDepartments().then(setDepartments).catch(() => setDepartments([]));
    void getProjects({ pageSize: 200 })
      .then((result) => setProjects(result.items))
      .catch(() => setProjects([]));
    void fetchEmployees().then(setEmployees).catch(() => setEmployees([]));
  }, []);

  const handlePresetChange = (value: DatePreset) => {
    setPreset(value);
    if (value !== 'custom') {
      setDatePreset(value);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Filters</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex flex-wrap items-end gap-4">
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Date preset</label>
            <Select
              value={preset}
              onChange={(e) => handlePresetChange(e.target.value as DatePreset)}
            >
              <option value="today">Today</option>
              <option value="week">This week</option>
              <option value="month">This month</option>
              <option value="custom">Custom</option>
            </Select>
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">From</label>
            <Input
              type="date"
              value={filters.fromDate ?? ''}
              onChange={(e) => {
                setPreset('custom');
                setFilters({ fromDate: e.target.value || undefined });
              }}
            />
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">To</label>
            <Input
              type="date"
              value={filters.toDate ?? ''}
              onChange={(e) => {
                setPreset('custom');
                setFilters({ toDate: e.target.value || undefined });
              }}
            />
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Department</label>
            <Select
              value={filters.departmentId ?? ''}
              onChange={(e) =>
                setFilters({ departmentId: e.target.value || undefined })
              }
            >
              <option value="">All departments</option>
              {departments.map((dept) => (
                <option key={dept.id} value={dept.id}>
                  {dept.name}
                </option>
              ))}
            </Select>
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Project</label>
            <Select
              value={filters.projectId ?? ''}
              onChange={(e) => setFilters({ projectId: e.target.value || undefined })}
            >
              <option value="">All projects</option>
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </Select>
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Employee</label>
            <Select
              value={filters.employeeId ?? ''}
              onChange={(e) => setFilters({ employeeId: e.target.value || undefined })}
            >
              <option value="">All employees</option>
              {employees.map((employee) => (
                <option key={employee.id} value={employee.id}>
                  {employee.firstName} {employee.lastName}
                </option>
              ))}
            </Select>
          </div>
          <Button
            variant="outline"
            onClick={() => {
              setPreset('month');
              setDatePreset('month');
              setFilters({
                departmentId: undefined,
                projectId: undefined,
                employeeId: undefined,
              });
            }}
          >
            Reset
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
