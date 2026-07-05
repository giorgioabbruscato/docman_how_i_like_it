import { apiClient } from '@/lib/api-client';
import type {
  AddProjectMemberRequest,
  CreateProjectRequest,
  ProjectDto,
  ProjectFilters,
  ProjectMemberDto,
  UpdateProjectRequest,
} from '@/types/project';
import type { PagedResult } from '@/types/common';

export async function getProjects(filters: ProjectFilters = {}): Promise<PagedResult<ProjectDto>> {
  const { data } = await apiClient.get<PagedResult<ProjectDto>>('/v1/projects', { params: filters });
  return data;
}

export async function getProject(id: string): Promise<ProjectDto> {
  const { data } = await apiClient.get<ProjectDto>(`/v1/projects/${id}`);
  return data;
}

export async function createProject(input: CreateProjectRequest): Promise<ProjectDto> {
  const { data } = await apiClient.post<ProjectDto>('/v1/projects', input);
  return data;
}

export async function updateProject(id: string, input: UpdateProjectRequest): Promise<ProjectDto> {
  const { data } = await apiClient.put<ProjectDto>(`/v1/projects/${id}`, input);
  return data;
}

export async function deleteProject(id: string): Promise<void> {
  await apiClient.delete(`/v1/projects/${id}`);
}

export async function getProjectMembers(projectId: string): Promise<ProjectMemberDto[]> {
  const { data } = await apiClient.get<ProjectMemberDto[]>(`/v1/projects/${projectId}/members`);
  return data;
}

export async function addProjectMember(
  projectId: string,
  input: AddProjectMemberRequest,
): Promise<ProjectMemberDto> {
  const { data } = await apiClient.post<ProjectMemberDto>(
    `/v1/projects/${projectId}/members`,
    input,
  );
  return data;
}

export async function removeProjectMember(projectId: string, memberId: string): Promise<void> {
  await apiClient.delete(`/v1/projects/${projectId}/members/${memberId}`);
}
