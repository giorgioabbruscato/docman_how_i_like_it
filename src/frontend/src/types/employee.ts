export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  jobTitle?: string;
  departmentId?: string;
  hireDate: string;
  isActive: boolean;
}

export interface CreateEmployeeInput {
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  jobTitle?: string;
  departmentId?: string;
}
