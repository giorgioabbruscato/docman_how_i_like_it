import type { PagedQuery } from '@/types/common';

export type TaskStatus = 'Todo' | 'InProgress' | 'Review' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export interface ProjectTaskDto {
  id: string;
  projectId: string;
  title: string;
  description?: string | null;
  assignedEmployeeId?: string | null;
  priority: TaskPriority;
  status: TaskStatus;
  estimatedHours?: number | null;
  spentHours: number;
  dueDate?: string | null;
}

export interface GetTasksQuery extends PagedQuery {
  search?: string;
  projectId?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedEmployeeId?: string;
}
