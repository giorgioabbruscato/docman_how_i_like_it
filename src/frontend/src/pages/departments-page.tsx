import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { createDepartment, deactivateDepartment, fetchDepartments } from '@/api/departments';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import type { Department } from '@/types/department';

const createDepartmentSchema = z.object({
  name: z.string().min(1, 'Required'),
  code: z
    .string()
    .min(1, 'Required')
    .regex(/^[A-Za-z0-9_-]+$/, 'Only letters, numbers, hyphens, and underscores'),
  description: z.string().optional(),
  parentDepartmentId: z.string().optional(),
});

type CreateDepartmentForm = z.infer<typeof createDepartmentSchema>;

export function DepartmentsPage() {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateDepartmentForm>({
    resolver: zodResolver(createDepartmentSchema),
  });

  const loadDepartments = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await fetchDepartments();
      setDepartments(data);
    } catch {
      setError('Failed to load departments. Ensure you are authenticated and the API is running.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDepartments();
  }, []);

  const onSubmit = async (data: CreateDepartmentForm) => {
    try {
      await createDepartment({
        name: data.name,
        code: data.code,
        description: data.description || undefined,
        parentDepartmentId: data.parentDepartmentId || undefined,
      });
      reset();
      await loadDepartments();
    } catch {
      setError('Failed to create department.');
    }
  };

  const handleDeactivate = async (id: string) => {
    try {
      await deactivateDepartment(id);
      await loadDepartments();
    } catch {
      setError('Failed to deactivate department. It may have active child departments.');
    }
  };

  const departmentName = (id?: string) =>
    departments.find((d) => d.id === id)?.name ?? '—';

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Departments</h2>
        <p className="text-muted-foreground">Organize your company structure by department.</p>
      </div>

      {error && (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Add Department</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div>
                <Input placeholder="Name" {...register('name')} />
                {errors.name && <p className="mt-1 text-xs text-red-600">{errors.name.message}</p>}
              </div>
              <div>
                <Input placeholder="Code (e.g. ENG)" {...register('code')} />
                {errors.code && <p className="mt-1 text-xs text-red-600">{errors.code.message}</p>}
              </div>
              <Input placeholder="Description" {...register('description')} />
              <div>
                <Select {...register('parentDepartmentId')}>
                  <option value="">No parent department</option>
                  {departments.map((dept) => (
                    <option key={dept.id} value={dept.id}>
                      {dept.name} ({dept.code})
                    </option>
                  ))}
                </Select>
              </div>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : 'Create Department'}
              </Button>
            </form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Department List</CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <p className="text-sm text-muted-foreground">Loading...</p>
            ) : departments.length === 0 ? (
              <p className="text-sm text-muted-foreground">No departments found.</p>
            ) : (
              <ul className="divide-y divide-border">
                {departments.map((department) => (
                  <li key={department.id} className="flex items-center justify-between py-3">
                    <div>
                      <p className="font-medium">
                        {department.name}{' '}
                        <span className="text-muted-foreground">({department.code})</span>
                      </p>
                      {department.parentDepartmentId && (
                        <p className="text-sm text-muted-foreground">
                          Parent: {departmentName(department.parentDepartmentId)}
                        </p>
                      )}
                      {department.description && (
                        <p className="text-sm text-muted-foreground">{department.description}</p>
                      )}
                    </div>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => handleDeactivate(department.id)}
                    >
                      Deactivate
                    </Button>
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
