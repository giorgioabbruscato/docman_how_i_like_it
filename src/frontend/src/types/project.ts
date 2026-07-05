import type { PagedQuery, PagedResult } from '@/types/common';

export type ProjectStatus = 'Active' | 'OnHold' | 'Completed' | 'Cancelled';

export type ProjectMemberRole = 'Lead' | 'Member' | 'Observer';

export interface ProjectDto {
  id: string;
  name: string;
  description?: string | null;
  customerName?: string | null;
  status: ProjectStatus;
  startDate?: string | null;
  endDate?: string | null;
  budgetHours?: number | null;
  budgetCost?: number | null;
  isArchived: boolean;
}

export interface ProjectMemberDto {
  id: string;
  projectId: string;
  employeeId: string;
  role: ProjectMemberRole;
  hourlyRate?: number | null;
}

export interface CreateProjectRequest {
  name: string;
  status?: ProjectStatus;
  description?: string | null;
  customerName?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  budgetHours?: number | null;
  budgetCost?: number | null;
}

export interface UpdateProjectRequest {
  name: string;
  status: ProjectStatus;
  description?: string | null;
  customerName?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  budgetHours?: number | null;
  budgetCost?: number | null;
}

export interface AddProjectMemberRequest {
  employeeId: string;
  role: ProjectMemberRole;
  hourlyRate?: number | null;
}

export interface ProjectFilters extends PagedQuery {
  search?: string;
  customerName?: string;
  status?: ProjectStatus;
  isArchived?: boolean;
}

export type { PagedResult };
