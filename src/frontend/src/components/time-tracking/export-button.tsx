import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { ErrorBanner } from '@/components/ui/loading-spinner';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { useExportTimeEntries } from '@/hooks/use-time-tracking';
import { getApiErrorMessage, todayDateString, triggerBlobDownload } from '@/lib/utils';
import type { ExportFormat } from '@/types/time-entry';

export function ExportButton() {
  const exportEntries = useExportTimeEntries();
  const [format, setFormat] = useState<ExportFormat>('csv');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState(todayDateString());
  const [error, setError] = useState<string | null>(null);

  const handleExport = async () => {
    try {
      setError(null);
      const blob = await exportEntries.mutateAsync({
        format,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
      });
      const extension = format === 'pdf' ? 'pdf' : format === 'xlsx' ? 'xlsx' : 'csv';
      triggerBlobDownload(blob, `time-entries.${extension}`);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to export time entries.'));
    }
  };

  return (
    <div className="space-y-3 rounded-md border border-border p-4">
      <p className="text-sm font-medium">Export Time Entries</p>
      {error && <ErrorBanner message={error} />}
      <div className="flex flex-wrap items-end gap-3">
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">Format</label>
          <Select value={format} onChange={(e) => setFormat(e.target.value as ExportFormat)}>
            <option value="csv">CSV</option>
            <option value="xlsx">XLSX</option>
            <option value="pdf">PDF</option>
          </Select>
        </div>
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">From</label>
          <Input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
        </div>
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">To</label>
          <Input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
        </div>
        <Button onClick={() => void handleExport()} disabled={exportEntries.isPending}>
          {exportEntries.isPending ? 'Exporting...' : 'Download'}
        </Button>
      </div>
    </div>
  );
}
