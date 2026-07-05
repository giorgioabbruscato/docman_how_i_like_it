import { apiClient } from '@/lib/api-client';

export interface UserNotification {
  id: string;
  type: string;
  title: string;
  body: string;
  metadataJson?: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface PagedNotifications {
  items: UserNotification[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export async function fetchNotifications(page = 1, pageSize = 10): Promise<PagedNotifications> {
  const { data } = await apiClient.get<PagedNotifications>('/v1/notifications', {
    params: { page, pageSize },
  });
  return data;
}

export async function markNotificationRead(id: string): Promise<UserNotification> {
  const { data } = await apiClient.patch<UserNotification>(`/v1/notifications/${id}/read`);
  return data;
}
