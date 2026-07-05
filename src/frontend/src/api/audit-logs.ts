import { apiClient } from '@/lib/api-client';
import type { AuditLogEntry, AuditLogQuery, PagedResult } from '@/types/audit-log';

export async function fetchAuditLogs(query: AuditLogQuery = {}): Promise<PagedResult<AuditLogEntry>> {
  const { data } = await apiClient.get<PagedResult<AuditLogEntry>>('/v1/audit-logs', {
    params: {
      from: query.from || undefined,
      to: query.to || undefined,
      actorUserId: query.actorUserId || undefined,
      action: query.action || undefined,
      decision: query.decision || undefined,
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 25,
    },
  });
  return data;
}
