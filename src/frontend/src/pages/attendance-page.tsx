import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { checkIn, checkOut, fetchAttendanceRecords, fetchAttendanceReport } from '@/api/attendance';
import { fetchEmployees } from '@/api/employees';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { Permission, hasAnyPermission, hasPermission } from '@/lib/auth-permissions';
import {
  currentTimeString,
  formatDate,
  getApiErrorMessage,
  todayDateString,
} from '@/lib/utils';
import { useAuthStore } from '@/stores/auth-store';
import type { AttendanceRecord, AttendanceReport } from '@/types/attendance';
import type { Employee } from '@/types/employee';

const checkInOutSchema = z.object({
  employeeId: z.string().min(1, 'Required'),
  date: z.string().min(1, 'Required'),
  time: z.string().min(1, 'Required'),
});

type CheckInOutForm = z.infer<typeof checkInOutSchema>;

const reportSchema = z
  .object({
    from: z.string().min(1, 'Required'),
    to: z.string().min(1, 'Required'),
  })
  .refine((data) => data.to >= data.from, {
    message: 'End date must be on or after start date',
    path: ['to'],
  });

type ReportForm = z.infer<typeof reportSchema>;

export function AttendancePage() {
  const permissions = useAuthStore((state) => state.permissions);
  const authEmployeeId = useAuthStore((state) => state.employeeId);

  const canCheckInOut = hasPermission(permissions, Permission.AttendanceWriteSelf);
  const canViewRecords = hasAnyPermission(
    permissions,
    Permission.AttendanceReadTenant,
    Permission.AttendanceReadTeam,
  );
  const canSelectEmployee = hasAnyPermission(
    permissions,
    Permission.EmployeeReadTenant,
    Permission.EmployeeReadTeam,
  );

  const [records, setRecords] = useState<AttendanceRecord[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [report, setReport] = useState<AttendanceReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const checkForm = useForm<CheckInOutForm>({
    resolver: zodResolver(checkInOutSchema),
    defaultValues: {
      employeeId: authEmployeeId ?? '',
      date: todayDateString(),
      time: currentTimeString(),
    },
  });

  const reportForm = useForm<ReportForm>({
    resolver: zodResolver(reportSchema),
    defaultValues: {
      from: todayDateString(),
      to: todayDateString(),
    },
  });

  useEffect(() => {
    if (!canSelectEmployee && authEmployeeId) {
      checkForm.setValue('employeeId', authEmployeeId);
    }
  }, [authEmployeeId, canSelectEmployee, checkForm]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      if (canViewRecords) {
        const [recordData, employeeData] = await Promise.all([
          fetchAttendanceRecords(),
          fetchEmployees(),
        ]);
        setRecords(recordData);
        setEmployees(employeeData);
      } else if (canSelectEmployee) {
        const employeeData = await fetchEmployees();
        setEmployees(employeeData);
      }
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to load attendance data.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [canViewRecords, canSelectEmployee]);

  const handleCheckIn = async (data: CheckInOutForm) => {
    try {
      setError(null);
      await checkIn({
        employeeId: data.employeeId,
        date: data.date,
        time: data.time,
      });
      checkForm.reset({
        date: todayDateString(),
        time: currentTimeString(),
        employeeId: canSelectEmployee ? data.employeeId : (authEmployeeId ?? data.employeeId),
      });
      if (canViewRecords) await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to check in.'));
    }
  };

  const handleCheckOut = async (data: CheckInOutForm) => {
    try {
      setError(null);
      await checkOut({
        employeeId: data.employeeId,
        date: data.date,
        time: data.time,
      });
      checkForm.reset({
        date: todayDateString(),
        time: currentTimeString(),
        employeeId: canSelectEmployee ? data.employeeId : (authEmployeeId ?? data.employeeId),
      });
      if (canViewRecords) await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to check out.'));
    }
  };

  const onReportSubmit = async (data: ReportForm) => {
    try {
      setError(null);
      const reportData = await fetchAttendanceReport(data.from, data.to);
      setReport(reportData);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to load attendance report.'));
    }
  };

  const employeeName = (id: string) => {
    const employee = employees.find((e) => e.id === id);
    return employee ? `${employee.firstName} ${employee.lastName}` : id;
  };

  const { register, handleSubmit, formState: { errors, isSubmitting } } = checkForm;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Attendance</h2>
        <p className="text-muted-foreground">Record check-in and check-out times.</p>
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="grid gap-6 lg:grid-cols-2">
        {canCheckInOut && (
          <Card>
            <CardHeader>
              <CardTitle>Check In / Check Out</CardTitle>
            </CardHeader>
            <CardContent>
              <form className="space-y-4">
                <div>
                  {canSelectEmployee ? (
                    <Select {...register('employeeId')}>
                      <option value="">Select employee</option>
                      {employees.map((employee) => (
                        <option key={employee.id} value={employee.id}>
                          {employee.firstName} {employee.lastName}
                        </option>
                      ))}
                    </Select>
                  ) : (
                    <Input type="hidden" {...register('employeeId')} />
                  )}
                  {!canSelectEmployee && authEmployeeId && (
                    <p className="text-sm text-muted-foreground">
                      Recording attendance for your employee record.
                    </p>
                  )}
                  {errors.employeeId && (
                    <p className="mt-1 text-xs text-red-600">{errors.employeeId.message}</p>
                  )}
                </div>
                <div className="grid gap-4 sm:grid-cols-2">
                  <Input type="date" {...register('date')} />
                  <Input type="time" step="1" {...register('time')} />
                </div>
                <div className="flex gap-2">
                  <Button
                    type="button"
                    disabled={isSubmitting}
                    onClick={handleSubmit(handleCheckIn)}
                  >
                    Check In
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    disabled={isSubmitting}
                    onClick={handleSubmit(handleCheckOut)}
                  >
                    Check Out
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        )}

        {canViewRecords && (
          <Card>
            <CardHeader>
              <CardTitle>Attendance Records</CardTitle>
            </CardHeader>
            <CardContent>
              {loading ? (
                <LoadingSpinner label="Loading attendance records" />
              ) : records.length === 0 ? (
                <EmptyState message="No attendance records found." />
              ) : (
                <ul className="divide-y divide-border">
                  {records.map((record) => (
                    <li key={record.id} className="py-3">
                      <p className="font-medium">{employeeName(record.employeeId)}</p>
                      <p className="text-sm text-muted-foreground">{formatDate(record.date)}</p>
                      <p className="text-sm text-muted-foreground">
                        {record.checkIn ?? '—'} → {record.checkOut ?? '—'} · {record.status}
                      </p>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        )}
      </div>

      {canViewRecords && (
        <Card>
          <CardHeader>
            <CardTitle>Attendance Report</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <form
              onSubmit={reportForm.handleSubmit(onReportSubmit)}
              className="flex flex-wrap items-end gap-4"
            >
              <div>
                <Input type="date" {...reportForm.register('from')} />
                {reportForm.formState.errors.from && (
                  <p className="mt-1 text-xs text-red-600">
                    {reportForm.formState.errors.from.message}
                  </p>
                )}
              </div>
              <div>
                <Input type="date" {...reportForm.register('to')} />
                {reportForm.formState.errors.to && (
                  <p className="mt-1 text-xs text-red-600">
                    {reportForm.formState.errors.to.message}
                  </p>
                )}
              </div>
              <Button type="submit">Generate Report</Button>
            </form>

            {report && (
              <div className="grid gap-4 sm:grid-cols-3 lg:grid-cols-6">
                <div className="rounded-md border p-3">
                  <p className="text-2xl font-bold">{report.totalRecords}</p>
                  <p className="text-xs text-muted-foreground">Total</p>
                </div>
                <div className="rounded-md border p-3">
                  <p className="text-2xl font-bold">{report.presentCount}</p>
                  <p className="text-xs text-muted-foreground">Present</p>
                </div>
                <div className="rounded-md border p-3">
                  <p className="text-2xl font-bold">{report.absentCount}</p>
                  <p className="text-xs text-muted-foreground">Absent</p>
                </div>
                <div className="rounded-md border p-3">
                  <p className="text-2xl font-bold">{report.lateCount}</p>
                  <p className="text-xs text-muted-foreground">Late</p>
                </div>
                <div className="rounded-md border p-3">
                  <p className="text-2xl font-bold">{report.halfDayCount}</p>
                  <p className="text-xs text-muted-foreground">Half Day</p>
                </div>
                <div className="rounded-md border p-3">
                  <p className="text-2xl font-bold">{report.remoteCount}</p>
                  <p className="text-xs text-muted-foreground">Remote</p>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
