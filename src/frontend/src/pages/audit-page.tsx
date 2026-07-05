import { useEffect, useState } from 'react';
import { fetchAuditLogs } from '@/api/audit-logs';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { EmptyState, ErrorBanner, LoadingSpinner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { formatDateTime, getApiErrorMessage } from '@/lib/utils';
import type { AuditLogEntry } from '@/types/audit-log';

const PAGE_SIZE = 25;

const DECISIONS = ['', 'Allow', 'Deny'] as const;

export function AuditPage() {
  const [entries, setEntries] = useState<AuditLogEntry[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [action, setAction] = useState('');
  const [decision, setDecision] = useState('');
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  const loadAuditLogs = async (targetPage: number) => {
    try {
      setLoading(true);
      setError(null);
      const result = await fetchAuditLogs({
        page: targetPage,
        pageSize: PAGE_SIZE,
        action: action || undefined,
        decision: decision || undefined,
        from: from ? new Date(from).toISOString() : undefined,
        to: to ? new Date(to).toISOString() : undefined,
      });
      setEntries(result.items);
      setTotalCount(result.totalCount);
      setPage(result.page);
    } catch (err) {
      setError(
        getApiErrorMessage(
          err,
          'Failed to load audit logs. This feature requires an Enterprise plan and the audit.read:tenant permission.',
        ),
      );
      setEntries([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadAuditLogs(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const onFilterSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    void loadAuditLogs(1);
  };

  const decisionBadgeClass = (value: string | null) =>
    value === 'Deny'
      ? 'bg-red-100 text-red-700'
      : value === 'Allow'
        ? 'bg-green-100 text-green-700'
        : 'bg-muted text-muted-foreground';

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Audit Logs</h2>
        <p className="text-muted-foreground">
          Enterprise access-decision history for this tenant.
        </p>
      </div>

      {error && <ErrorBanner message={error} />}

      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={onFilterSubmit} className="flex flex-wrap items-end gap-4">
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">Action</label>
              <Input
                placeholder="e.g. employee.read"
                value={action}
                onChange={(e) => setAction(e.target.value)}
              />
            </div>
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">Decision</label>
              <Select value={decision} onChange={(e) => setDecision(e.target.value)}>
                {DECISIONS.map((value) => (
                  <option key={value} value={value}>
                    {value || 'All'}
                  </option>
                ))}
              </Select>
            </div>
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">From</label>
              <Input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
            </div>
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">To</label>
              <Input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
            </div>
            <Button type="submit">Apply</Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Entries</CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <LoadingSpinner label="Loading audit logs" />
          ) : entries.length === 0 ? (
            <EmptyState message="No audit log entries found." />
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border text-left text-xs uppercase text-muted-foreground">
                      <th className="py-2 pr-4">Timestamp</th>
                      <th className="py-2 pr-4">Actor</th>
                      <th className="py-2 pr-4">Action</th>
                      <th className="py-2 pr-4">Entity</th>
                      <th className="py-2 pr-4">Scope</th>
                      <th className="py-2 pr-4">Decision</th>
                      <th className="py-2 pr-4">IP Address</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {entries.map((entry) => (
                      <tr key={entry.id}>
                        <td className="py-2 pr-4 whitespace-nowrap">
                          {formatDateTime(entry.timestamp)}
                        </td>
                        <td className="py-2 pr-4">{entry.actorEmail ?? entry.userId}</td>
                        <td className="py-2 pr-4">{entry.action}</td>
                        <td className="py-2 pr-4">
                          {entry.entity}
                          {entry.targetId ? ` (${entry.targetId})` : ''}
                        </td>
                        <td className="py-2 pr-4">{entry.scope ?? '—'}</td>
                        <td className="py-2 pr-4">
                          <span
                            className={`rounded-full px-2 py-0.5 text-xs ${decisionBadgeClass(entry.decision)}`}
                          >
                            {entry.decision ?? '—'}
                          </span>
                        </td>
                        <td className="py-2 pr-4">{entry.ipAddress ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="mt-4 flex items-center justify-between">
                <p className="text-xs text-muted-foreground">
                  Page {page} of {totalPages} · {totalCount} total
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => void loadAuditLogs(page - 1)}
                  >
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= totalPages}
                    onClick={() => void loadAuditLogs(page + 1)}
                  >
                    Next
                  </Button>
                </div>
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
