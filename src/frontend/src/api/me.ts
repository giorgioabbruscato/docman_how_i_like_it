import { apiClient } from '@/lib/api-client';
import type { Me } from '@/types/me';

export async function fetchMe(): Promise<Me> {
  const { data } = await apiClient.get<Me>('/v1/me');
  return data;
}
