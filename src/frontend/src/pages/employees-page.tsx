import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { fetchDepartments } from '@/api/departments';
import { createEmployee, deactivateEmployee, fetchEmployees } from '@/api/employees';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { Permission, hasPermission } from '@/lib/auth-permissions';
import { confirmAction, getApiErrorMessage } from '@/lib/utils';
import { useAuthStore } from '@/stores/auth-store';
import type { Department } from '@/types/department';
import type { Employee } from '@/types/employee';

const createEmployeeSchema = z.object({
  firstName: z.string().min(1, 'Required'),
  lastName: z.string().min(1, 'Required'),
  email: z.string().email('Invalid email'),
  hireDate: z.string().min(1, 'Required'),
  jobTitle: z.string().optional(),
  departmentId: z.string().optional(),
});

type CreateEmployeeForm = z.infer<typeof createEmployeeSchema>;

export function EmployeesPage() {
  const permissions = useAuthStore((state) => state.permissions);
  const canCreateEmployee = hasPermission(permissions, Permission.EmployeeCreateTenant);
  const canDeactivateEmployee = hasPermission(permissions, Permission.EmployeeDeleteTenant);

  const [employees, setEmployees] = useState<Employee[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateEmployeeForm>({
    resolver: zodResolver(createEmployeeSchema),
    defaultValues: {
      hireDate: new Date().toISOString().slice(0, 10),
    },
  });

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const [employeeData, departmentData] = await Promise.all([
        fetchEmployees(),
        fetchDepartments(),
      ]);
      setEmployees(employeeData);
      setDepartments(departmentData);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to load data. Ensure you are authenticated and the API is running.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const onSubmit = async (data: CreateEmployeeForm) => {
    try {
      await createEmployee({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        hireDate: data.hireDate,
        jobTitle: data.jobTitle || undefined,
        departmentId: data.departmentId || undefined,
      });
      reset({ hireDate: new Date().toISOString().slice(0, 10) });
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to create employee.'));
    }
  };

  const handleDeactivate = async (id: string) => {
    if (!confirmAction('Deactivate this employee?')) return;
    try {
      await deactivateEmployee(id);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to deactivate employee.'));
    }
  };

  const departmentName = (id?: string) =>
    departments.find((d) => d.id === id)?.name;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Employees</h2>
        <p className="text-muted-foreground">Manage employee records for your organization.</p>
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="grid gap-6 lg:grid-cols-2">
        {canCreateEmployee && (
        <Card>
          <CardHeader>
            <CardTitle>Add Employee</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <Input placeholder="First name" {...register('firstName')} />
                  {errors.firstName && (
                    <p className="mt-1 text-xs text-red-600">{errors.firstName.message}</p>
                  )}
                </div>
                <div>
                  <Input placeholder="Last name" {...register('lastName')} />
                  {errors.lastName && (
                    <p className="mt-1 text-xs text-red-600">{errors.lastName.message}</p>
                  )}
                </div>
              </div>
              <Input placeholder="Email" type="email" {...register('email')} />
              {errors.email && <p className="text-xs text-red-600">{errors.email.message}</p>}
              <Input placeholder="Job title" {...register('jobTitle')} />
              <Select {...register('departmentId')}>
                <option value="">No department</option>
                {departments.map((dept) => (
                  <option key={dept.id} value={dept.id}>
                    {dept.name} ({dept.code})
                  </option>
                ))}
              </Select>
              <Input type="date" {...register('hireDate')} />
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : 'Create Employee'}
              </Button>
            </form>
          </CardContent>
        </Card>
        )}

        <Card>
          <CardHeader>
            <CardTitle>Employee List</CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <LoadingSpinner label="Loading employees" />
            ) : employees.length === 0 ? (
              <EmptyState message="No employees found." />
            ) : (
              <ul className="divide-y divide-border">
                {employees.map((employee) => (
                  <li key={employee.id} className="flex items-center justify-between py-3">
                    <div>
                      <p className="font-medium">
                        {employee.firstName} {employee.lastName}
                      </p>
                      <p className="text-sm text-muted-foreground">{employee.email}</p>
                      {employee.departmentId && (
                        <p className="text-sm text-muted-foreground">
                          {departmentName(employee.departmentId) ?? 'Unknown department'}
                        </p>
                      )}
                    </div>
                    {canDeactivateEmployee && (
                      <Button
                        variant="destructive"
                        size="sm"
                        onClick={() => handleDeactivate(employee.id)}
                      >
                        Deactivate
                      </Button>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
