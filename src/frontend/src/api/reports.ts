import { apiClient } from '@/lib/api-client';
import type { ReportDownloadParams, ReportType } from '@/types/analytics';

export async function downloadReport(
  type: ReportType,
  params: ReportDownloadParams,
): Promise<Blob> {
  const { format, departmentId, projectId, employeeId, fromDate, toDate } = params;
  const { data } = await apiClient.get<Blob>(`/v1/reports/${type}`, {
    params: {
      format,
      departmentId,
      projectId,
      employeeId,
      fromDate,
      toDate,
    },
    responseType: 'blob',
  });
  return data;
}
