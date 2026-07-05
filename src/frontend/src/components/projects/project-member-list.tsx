import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { fetchEmployees } from '@/api/employees';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import {
  useAddProjectMember,
  useProjectMembers,
  useRemoveProjectMember,
} from '@/hooks/use-projects';
import { confirmAction, getApiErrorMessage } from '@/lib/utils';
import type { Employee } from '@/types/employee';
import type { ProjectMemberDto, ProjectMemberRole } from '@/types/project';

const addMemberSchema = z.object({
  employeeId: z.string().min(1, 'Required'),
  role: z.enum(['Lead', 'Member', 'Observer']),
  hourlyRate: z.string().optional(),
});

type AddMemberForm = z.infer<typeof addMemberSchema>;

interface ProjectMemberListProps {
  projectId: string;
  canManageMembers: boolean;
}

export function ProjectMemberList({ projectId, canManageMembers }: ProjectMemberListProps) {
  const { data: members = [], isLoading, error } = useProjectMembers(projectId);
  const addMember = useAddProjectMember();
  const removeMember = useRemoveProjectMember();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [employeesLoading, setEmployeesLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AddMemberForm>({
    resolver: zodResolver(addMemberSchema),
    defaultValues: { role: 'Member' },
  });

  useEffect(() => {
    if (!canManageMembers) return;
    setEmployeesLoading(true);
    fetchEmployees()
      .then(setEmployees)
      .catch(() => setEmployees([]))
      .finally(() => setEmployeesLoading(false));
  }, [canManageMembers]);

  const employeeName = (id: string) => {
    const employee = employees.find((e) => e.id === id);
    return employee ? `${employee.firstName} ${employee.lastName}` : id;
  };

  const onAddMember = async (data: AddMemberForm) => {
    try {
      setFormError(null);
      await addMember.mutateAsync({
        projectId,
        input: {
          employeeId: data.employeeId,
          role: data.role as ProjectMemberRole,
          hourlyRate: data.hourlyRate ? Number(data.hourlyRate) : undefined,
        },
      });
      reset({ role: 'Member', employeeId: '', hourlyRate: '' });
    } catch (err) {
      setFormError(getApiErrorMessage(err, 'Failed to add member.'));
    }
  };

  const onRemoveMember = async (member: ProjectMemberDto) => {
    if (!confirmAction('Remove this member from the project?')) return;
    try {
      setFormError(null);
      await removeMember.mutateAsync({ projectId, memberId: member.id });
    } catch (err) {
      setFormError(getApiErrorMessage(err, 'Failed to remove member.'));
    }
  };

  return (
    <div className="space-y-6">
      {formError && <ErrorBanner message={formError} />}
      {error && (
        <ErrorBanner message={getApiErrorMessage(error, 'Failed to load project members.')} />
      )}

      <Card>
        <CardHeader>
          <CardTitle>Members</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <LoadingSpinner label="Loading members" />
          ) : members.length === 0 ? (
            <EmptyState message="No members assigned to this project." />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-xs uppercase text-muted-foreground">
                    <th className="py-2 pr-4">Employee</th>
                    <th className="py-2 pr-4">Role</th>
                    <th className="py-2 pr-4">Hourly Rate</th>
                    {canManageMembers && <th className="py-2 pr-4">Actions</th>}
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {members.map((member) => (
                    <tr key={member.id}>
                      <td className="py-2 pr-4">{employeeName(member.employeeId)}</td>
                      <td className="py-2 pr-4">{member.role}</td>
                      <td className="py-2 pr-4">
                        {member.hourlyRate != null ? `$${member.hourlyRate}` : '—'}
                      </td>
                      {canManageMembers && (
                        <td className="py-2 pr-4">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => void onRemoveMember(member)}
                            disabled={removeMember.isPending}
                          >
                            Remove
                          </Button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {canManageMembers && (
        <Card>
          <CardHeader>
            <CardTitle>Add Member</CardTitle>
          </CardHeader>
          <CardContent>
            {employeesLoading ? (
              <LoadingSpinner label="Loading employees" />
            ) : (
              <form onSubmit={handleSubmit(onAddMember)} className="space-y-4">
                <Select {...register('employeeId')}>
                  <option value="">Select employee</option>
                  {employees.map((employee) => (
                    <option key={employee.id} value={employee.id}>
                      {employee.firstName} {employee.lastName}
                    </option>
                  ))}
                </Select>
                {errors.employeeId && (
                  <p className="text-xs text-red-600">{errors.employeeId.message}</p>
                )}
                <Select {...register('role')}>
                  <option value="Lead">Lead</option>
                  <option value="Member">Member</option>
                  <option value="Observer">Observer</option>
                </Select>
                <Input
                  type="number"
                  step="0.01"
                  placeholder="Hourly rate (optional)"
                  {...register('hourlyRate')}
                />
                <Button type="submit" disabled={isSubmitting || addMember.isPending}>
                  {isSubmitting ? 'Adding...' : 'Add Member'}
                </Button>
              </form>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
