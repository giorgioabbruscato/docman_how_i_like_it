export interface AuditLogEntry {
  id: string;
  timestamp: string;
  userId: string;
  actorEmail: string | null;
  action: string;
  entity: string;
  entityId: string | null;
  targetId: string | null;
  scope: string | null;
  decision: string | null;
  ipAddress: string | null;
  metadata: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AuditLogQuery {
  from?: string;
  to?: string;
  actorUserId?: string;
  action?: string;
  decision?: string;
  page?: number;
  pageSize?: number;
}
