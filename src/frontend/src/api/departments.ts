import { apiClient } from '@/lib/api-client';
import type { CreateDepartmentInput, Department } from '@/types/department';

export async function fetchDepartments(): Promise<Department[]> {
  const { data } = await apiClient.get<Department[]>('/v1/departments');
  return data;
}

export async function createDepartment(input: CreateDepartmentInput): Promise<Department> {
  const { data } = await apiClient.post<Department>('/v1/departments', input);
  return data;
}

export async function deactivateDepartment(id: string): Promise<void> {
  await apiClient.delete(`/v1/departments/${id}`);
}
