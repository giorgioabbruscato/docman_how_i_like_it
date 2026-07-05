import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import type { CreateProjectRequest, ProjectDto, ProjectStatus } from '@/types/project';

const projectSchema = z.object({
  name: z.string().min(1, 'Required'),
  status: z.enum(['Active', 'OnHold', 'Completed', 'Cancelled']),
  description: z.string().optional(),
  customerName: z.string().optional(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  budgetHours: z.string().optional(),
  budgetCost: z.string().optional(),
});

type ProjectFormValues = z.infer<typeof projectSchema>;

interface ProjectFormProps {
  project?: ProjectDto;
  onSubmit: (data: CreateProjectRequest) => Promise<void>;
  submitLabel?: string;
}

function toFormValues(project?: ProjectDto): ProjectFormValues {
  return {
    name: project?.name ?? '',
    status: project?.status ?? 'Active',
    description: project?.description ?? '',
    customerName: project?.customerName ?? '',
    startDate: project?.startDate ?? '',
    endDate: project?.endDate ?? '',
    budgetHours: project?.budgetHours != null ? String(project.budgetHours) : '',
    budgetCost: project?.budgetCost != null ? String(project.budgetCost) : '',
  };
}

function toRequest(data: ProjectFormValues): CreateProjectRequest {
  return {
    name: data.name,
    status: data.status as ProjectStatus,
    description: data.description || undefined,
    customerName: data.customerName || undefined,
    startDate: data.startDate || undefined,
    endDate: data.endDate || undefined,
    budgetHours: data.budgetHours ? Number(data.budgetHours) : undefined,
    budgetCost: data.budgetCost ? Number(data.budgetCost) : undefined,
  };
}

export function ProjectForm({ project, onSubmit, submitLabel = 'Save Project' }: ProjectFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ProjectFormValues>({
    resolver: zodResolver(projectSchema),
    defaultValues: toFormValues(project),
  });

  const handleFormSubmit = async (data: ProjectFormValues) => {
    await onSubmit(toRequest(data));
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
      <div>
        <Input placeholder="Project name" {...register('name')} />
        {errors.name && <p className="mt-1 text-xs text-red-600">{errors.name.message}</p>}
      </div>
      <Select {...register('status')}>
        <option value="Active">Active</option>
        <option value="OnHold">On Hold</option>
        <option value="Completed">Completed</option>
        <option value="Cancelled">Cancelled</option>
      </Select>
      <Input placeholder="Description" {...register('description')} />
      <Input placeholder="Customer name" {...register('customerName')} />
      <div className="grid gap-4 sm:grid-cols-2">
        <Input type="date" {...register('startDate')} />
        <Input type="date" {...register('endDate')} />
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        <Input type="number" step="0.01" placeholder="Budget hours" {...register('budgetHours')} />
        <Input type="number" step="0.01" placeholder="Budget cost" {...register('budgetCost')} />
      </div>
      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving...' : submitLabel}
      </Button>
    </form>
  );
}
