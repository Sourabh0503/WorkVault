import { EmployeeStatus } from './employee.models';

/**
 * Current-user profile returned by GET /api/auth/me.
 * Mirrors CurrentUserDto / EmployeeProfileDto on the backend.
 */
export interface CurrentUserProfile {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roleName: string;
  companyId: string;
  companyName: string;
  employee: EmployeeProfile | null;
}

export interface EmployeeProfile {
  employeeCode: string;
  phone: string | null;
  dateOfBirth: string | null; // "yyyy-MM-dd"
  joinDate: string; // "yyyy-MM-dd"
  departmentName: string | null;
  designationTitle: string | null;
  status: EmployeeStatus;
  manager: ManagerSummary | null;
}

export interface ManagerSummary {
  id: string;
  employeeCode: string;
  fullName: string;
}
