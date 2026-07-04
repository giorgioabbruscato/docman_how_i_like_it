import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { fetchEmployees } from '@/api/employees';
import {
  approveLeaveRequest,
  cancelLeaveRequest,
  createLeaveRequest,
  fetchLeaveRequests,
  rejectLeaveRequest,
} from '@/api/leave-requests';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { hasAnyRole, MANAGER_OR_ABOVE_ROLES } from '@/lib/auth-roles';
import {
  confirmAction,
  formatDate,
  getApiErrorMessage,
  todayDateString,
} from '@/lib/utils';
import { useAuthStore } from '@/stores/auth-store';
import { LEAVE_TYPES } from '@/types/leave-request';
import type { LeaveRequest } from '@/types/leave-request';
import type { Employee } from '@/types/employee';

const createLeaveSchema = z
  .object({
    employeeId: z.string().min(1, 'Required'),
    startDate: z.string().min(1, 'Required'),
    endDate: z.string().min(1, 'Required'),
    type: z.string().min(1, 'Required'),
    reason: z.string().optional(),
  })
  .refine((data) => data.endDate >= data.startDate, {
    message: 'End date must be on or after start date',
    path: ['endDate'],
  });

type CreateLeaveForm = z.infer<typeof createLeaveSchema>;

export function LeaveRequestsPage() {
  const user = useAuthStore((state) => state.user);
  const isManagerOrAbove = hasAnyRole(user?.roles ?? [], ...MANAGER_OR_ABOVE_ROLES);

  const [requests, setRequests] = useState<LeaveRequest[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [rejectingId, setRejectingId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateLeaveForm>({
    resolver: zodResolver(createLeaveSchema),
    defaultValues: {
      startDate: todayDateString(),
      endDate: todayDateString(),
      type: 'Annual',
    },
  });

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      if (isManagerOrAbove) {
        const [requestData, employeeData] = await Promise.all([
          fetchLeaveRequests(),
          fetchEmployees(),
        ]);
        setRequests(requestData);
        setEmployees(employeeData);
      } else if (employees.length === 0) {
        setRequests([]);
      }
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to load leave requests.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [isManagerOrAbove]);

  const onSubmit = async (data: CreateLeaveForm) => {
    try {
      setError(null);
      await createLeaveRequest({
        employeeId: data.employeeId,
        startDate: data.startDate,
        endDate: data.endDate,
        type: data.type,
        reason: data.reason || undefined,
      });
      reset({
        employeeId: '',
        startDate: todayDateString(),
        endDate: todayDateString(),
        type: 'Annual',
        reason: '',
      });
      if (isManagerOrAbove) {
        await loadData();
      }
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to create leave request.'));
    }
  };

  const handleApprove = async (id: string) => {
    if (!confirmAction('Approve this leave request?')) return;
    try {
      setError(null);
      await approveLeaveRequest(id);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to approve leave request.'));
    }
  };

  const handleReject = async (id: string) => {
    if (!confirmAction('Reject this leave request?')) return;
    try {
      setError(null);
      await rejectLeaveRequest(id, { reason: rejectReason || undefined });
      setRejectingId(null);
      setRejectReason('');
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to reject leave request.'));
    }
  };

  const handleCancel = async (id: string) => {
    if (!confirmAction('Cancel this leave request?')) return;
    try {
      setError(null);
      await cancelLeaveRequest(id);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to cancel leave request.'));
    }
  };

  const employeeName = (id: string) => {
    const employee = employees.find((e) => e.id === id);
    return employee ? `${employee.firstName} ${employee.lastName}` : id;
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Leave Requests</h2>
        <p className="text-muted-foreground">Submit and manage employee leave requests.</p>
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Request Leave</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div>
                {isManagerOrAbove ? (
                  <Select {...register('employeeId')}>
                    <option value="">Select employee</option>
                    {employees.map((employee) => (
                      <option key={employee.id} value={employee.id}>
                        {employee.firstName} {employee.lastName}
                      </option>
                    ))}
                  </Select>
                ) : (
                  <>
                    <Input placeholder="Employee ID (UUID)" {...register('employeeId')} />
                    <p className="mt-1 text-xs text-muted-foreground">
                      Enter your employee record ID. Contact HR if you do not have it.
                    </p>
                  </>
                )}
                {errors.employeeId && (
                  <p className="mt-1 text-xs text-red-600">{errors.employeeId.message}</p>
                )}
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <Input type="date" {...register('startDate')} />
                  {errors.startDate && (
                    <p className="mt-1 text-xs text-red-600">{errors.startDate.message}</p>
                  )}
                </div>
                <div>
                  <Input type="date" {...register('endDate')} />
                  {errors.endDate && (
                    <p className="mt-1 text-xs text-red-600">{errors.endDate.message}</p>
                  )}
                </div>
              </div>
              <Select {...register('type')}>
                {LEAVE_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </Select>
              <Input placeholder="Reason (optional)" {...register('reason')} />
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Submitting...' : 'Submit Request'}
              </Button>
            </form>
          </CardContent>
        </Card>

        {isManagerOrAbove && (
          <Card>
            <CardHeader>
              <CardTitle>Leave Request List</CardTitle>
            </CardHeader>
            <CardContent>
              {loading ? (
                <LoadingSpinner label="Loading leave requests" />
              ) : requests.length === 0 ? (
                <EmptyState message="No leave requests found." />
              ) : (
                <ul className="divide-y divide-border">
                  {requests.map((request) => (
                    <li key={request.id} className="space-y-2 py-3">
                      <div className="flex items-start justify-between gap-2">
                        <div>
                          <p className="font-medium">{employeeName(request.employeeId)}</p>
                          <p className="text-sm text-muted-foreground">
                            {formatDate(request.startDate)} – {formatDate(request.endDate)}
                          </p>
                          <p className="text-sm text-muted-foreground">
                            {request.type} · {request.status}
                          </p>
                          {request.reason && (
                            <p className="text-sm text-muted-foreground">{request.reason}</p>
                          )}
                        </div>
                        <span className="rounded-full bg-muted px-2 py-0.5 text-xs">
                          {request.status}
                        </span>
                      </div>
                      {request.status === 'Pending' && (
                        <div className="flex flex-wrap gap-2">
                          <Button size="sm" onClick={() => handleApprove(request.id)}>
                            Approve
                          </Button>
                          {rejectingId === request.id ? (
                            <div className="flex flex-1 flex-wrap items-center gap-2">
                              <Input
                                placeholder="Rejection reason (optional)"
                                value={rejectReason}
                                onChange={(e) => setRejectReason(e.target.value)}
                                className="flex-1"
                              />
                              <Button size="sm" variant="destructive" onClick={() => handleReject(request.id)}>
                                Confirm Reject
                              </Button>
                              <Button
                                size="sm"
                                variant="outline"
                                onClick={() => {
                                  setRejectingId(null);
                                  setRejectReason('');
                                }}
                              >
                                Cancel
                              </Button>
                            </div>
                          ) : (
                            <Button
                              size="sm"
                              variant="destructive"
                              onClick={() => setRejectingId(request.id)}
                            >
                              Reject
                            </Button>
                          )}
                          <Button size="sm" variant="outline" onClick={() => handleCancel(request.id)}>
                            Cancel Request
                          </Button>
                        </div>
                      )}
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
