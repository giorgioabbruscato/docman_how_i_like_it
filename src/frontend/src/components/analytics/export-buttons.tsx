import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorBanner } from '@/components/ui/loading-spinner';
import { Select } from '@/components/ui/select';
import { useDownloadReport } from '@/hooks/use-analytics';
import { Permission, hasAnyPermission } from '@/lib/auth-permissions';
import { getApiErrorMessage, triggerBlobDownload } from '@/lib/utils';
import { useAuthStore } from '@/stores/auth-store';
import type { AnalyticsFilters, ReportFormat, ReportType } from '@/types/analytics';

interface ExportButtonsProps {
  filters: AnalyticsFilters;
}

const REPORT_TYPES: { type: ReportType; label: string }[] = [
  { type: 'attendance', label: 'Attendance' },
  { type: 'worked-hours', label: 'Worked Hours' },
  { type: 'employees', label: 'Employees' },
  { type: 'projects', label: 'Projects' },
];

export function ExportButtons({ filters }: ExportButtonsProps) {
  const permissions = useAuthStore((state) => state.permissions);
  const canExport = hasAnyPermission(
    permissions,
    Permission.ReportGenerateSelf,
    Permission.ReportGenerateTeam,
    Permission.ReportGenerateTenant,
  );

  const downloadReport = useDownloadReport();
  const [format, setFormat] = useState<ReportFormat>('csv');
  const [error, setError] = useState<string | null>(null);

  if (!canExport) {
    return null;
  }

  const handleExport = async (type: ReportType) => {
    try {
      setError(null);
      const blob = await downloadReport.mutateAsync({
        type,
        params: { ...filters, format },
      });
      const extension = format === 'pdf' ? 'pdf' : format === 'xlsx' ? 'xlsx' : 'csv';
      triggerBlobDownload(blob, `${type}-report.${extension}`);
    } catch (err) {
      setError(getApiErrorMessage(err, `Failed to export ${type} report.`));
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Export Reports</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {error && <ErrorBanner message={error} />}
        <div className="flex flex-wrap items-end gap-4">
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Format</label>
            <Select value={format} onChange={(e) => setFormat(e.target.value as ReportFormat)}>
              <option value="csv">CSV</option>
              <option value="xlsx">XLSX</option>
              <option value="pdf">PDF</option>
            </Select>
          </div>
          <div className="flex flex-wrap gap-2">
            {REPORT_TYPES.map(({ type, label }) => (
              <Button
                key={type}
                variant="outline"
                disabled={downloadReport.isPending}
                onClick={() => void handleExport(type)}
              >
                {downloadReport.isPending ? 'Exporting…' : `Export ${label}`}
              </Button>
            ))}
          </div>
        </div>
        <p className="text-xs text-muted-foreground">
          Exports use the current filter selection (department, project, employee, date range).
        </p>
      </CardContent>
    </Card>
  );
}
