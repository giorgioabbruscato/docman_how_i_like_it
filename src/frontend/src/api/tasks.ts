import { apiClient } from '@/lib/api-client';
import type { PagedResult } from '@/types/common';
import type { GetTasksQuery, ProjectTaskDto } from '@/types/task';

export async function getTasks(query: GetTasksQuery = {}): Promise<PagedResult<ProjectTaskDto>> {
  const { data } = await apiClient.get<PagedResult<ProjectTaskDto>>('/v1/tasks', { params: query });
  return data;
}
