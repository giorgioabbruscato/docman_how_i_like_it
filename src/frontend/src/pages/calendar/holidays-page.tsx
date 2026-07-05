import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createHoliday, deleteHoliday, fetchHolidays } from '@/api/calendar';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { LoadingSpinner } from '@/components/ui/loading-spinner';
import { Permission, useHasPermission } from '@/lib/auth-permissions';
import { useState } from 'react';

export function HolidaysPage() {
  const canManage = useHasPermission(Permission.CalendarManageTenant);
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: ['holidays'],
    queryFn: fetchHolidays,
    enabled: canManage,
  });
  const [name, setName] = useState('');
  const [date, setDate] = useState('');

  const createMutation = useMutation({
    mutationFn: createHoliday,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['holidays'] }),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteHoliday,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['holidays'] }),
  });

  if (!canManage) {
    return <p className="text-muted-foreground">You do not have permission to manage holidays.</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Public Holidays</h2>
      </div>
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Add holiday</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          <Input placeholder="Name" value={name} onChange={(e) => setName(e.target.value)} />
          <Input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          <Button
            onClick={() => void createMutation.mutateAsync({ name, date })}
            disabled={!name || !date}
          >
            Add
          </Button>
        </CardContent>
      </Card>
      {isLoading && <LoadingSpinner label="Loading holidays" />}
      <div className="grid gap-3">
        {data?.map((holiday) => (
          <Card key={holiday.id}>
            <CardContent className="flex items-center justify-between py-4">
              <span>
                {holiday.name} — {holiday.date}
              </span>
              <Button
                size="sm"
                variant="outline"
                onClick={() => void deleteMutation.mutateAsync(holiday.id)}
              >
                Delete
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
