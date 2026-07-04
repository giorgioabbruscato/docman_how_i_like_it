import { apiClient } from '@/lib/api-client';
import type { CreateEmployeeInput, Employee } from '@/types/employee';

export async function fetchEmployees(): Promise<Employee[]> {
  const { data } = await apiClient.get<Employee[]>('/v1/employees');
  return data;
}

export async function fetchEmployee(id: string): Promise<Employee> {
  const { data } = await apiClient.get<Employee>(`/v1/employees/${id}`);
  return data;
}

export async function createEmployee(input: CreateEmployeeInput): Promise<Employee> {
  const { data } = await apiClient.post<Employee>('/v1/employees', input);
  return data;
}

export async function deactivateEmployee(id: string): Promise<void> {
  await apiClient.delete(`/v1/employees/${id}`);
}
