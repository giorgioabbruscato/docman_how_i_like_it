export interface Department {
  id: string;
  name: string;
  code: string;
  description?: string;
  parentDepartmentId?: string;
  isActive: boolean;
}

export interface CreateDepartmentInput {
  name: string;
  code: string;
  description?: string;
  parentDepartmentId?: string;
}
