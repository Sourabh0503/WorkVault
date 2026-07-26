/**
 * Employee domain types shared across the employee feature — status enum/labels, list
 * rows, query params, my-team shapes, and the create/detail/update request+response
 * DTOs. Each interface mirrors its backend counterpart (noted inline).
 */

// Mirrors EmployeeStatus enum (WorkVault.Domain).
export enum EmployeeStatus {
  Pending = 0,
  Active = 1,
  OnNotice = 2,
  Suspended = 3,
  Offboarded = 4,
}

export const EMPLOYEE_STATUS_LABELS: Record<EmployeeStatus, string> = {
  [EmployeeStatus.Pending]: 'Pending',
  [EmployeeStatus.Active]: 'Active',
  [EmployeeStatus.OnNotice]: 'On Notice',
  [EmployeeStatus.Suspended]: 'Suspended',
  [EmployeeStatus.Offboarded]: 'Offboarded',
};

// Mirrors EmployeeListDto (WorkVault.Application).
export interface EmployeeListItem {
  id: string;
  employeeCode: string;
  fullName: string;
  email: string;
  departmentName: string | null;
  designationTitle: string | null;
  status: EmployeeStatus;
  joinDate: string; // DateOnly serializes as "yyyy-MM-dd"
}

// Query params for GET /api/employees.
export interface EmployeeListQuery {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: EmployeeStatus;
}

// Matches TeamMemberDto from GetMyTeamHandler.cs
export interface TeamMember {
  id: string;
  employeeCode: string;
  fullName: string;
  email: string;
  phone: string | null;
  photoUrl: string | null;
  designationTitle: string | null;
  isMe: boolean;
}

// Matches MyTeamResult
export interface MyTeamResult {
  departmentName: string | null;
  members: TeamMember[];
}

// POST /api/employees — request body
export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  joinDate: string;        // "yyyy-MM-dd"
  departmentId?: string;
  designationId?: string;
  managerId?: string;
  roleId: string;
}

// POST /api/employees — response
export interface CreateEmployeeResult {
  employeeId: string;
  employeeCode: string;
  inviteLink: string;
}

//GET api/employees/{id} — response
// Nested info objects (match backend EmployeeDto)
export interface DepartmentInfo {
  id: string;
  name: string;
}

export interface DesignationInfo {
  id: string;
  title: string;
  level: number;
}

export interface ManagerInfo {
  id: string;
  employeeCode: string;
  fullName: string;
}

export interface RoleInfo {
  id: string;
  name: string;
}

// GET /api/employees/managers?departmentId= — dropdown option (matches ManagerOptionDto)
export interface ManagerOption {
  id: string;
  fullName: string;
  employeeCode: string;
}

// Matches EmployeeDto from GetEmployeeByIdHandler.cs
export interface EmployeeDetail {
  id: string;
  employeeCode: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string | null;
  photoUrl: string | null;
  dateOfBirth: string | null;
  joinDate: string;
  resignationDate: string | null;
  lastWorkingDay: string | null;
  status: EmployeeStatus;
  department: DepartmentInfo | null;
  designation: DesignationInfo | null;
  manager: ManagerInfo | null;
  role: RoleInfo;
  createdAt: string;
}

// Matches UpdateEmployeeCommand (backend)
export interface UpdateEmployeeRequest {
  id: string;
  firstName: string;
  lastName: string;
  phone?: string;
  photoUrl?: string;
  dateOfBirth?: string;
  departmentId?: string;
  designationId?: string;
  managerId?: string;
  roleId: string;
  joinDate: string;
  resignationDate?: string;
  lastWorkingDay?: string;
  status: EmployeeStatus;
}

// Matches UpdateEmployeeResult
export interface UpdateEmployeeResult {
  id: string;
  employeeCode: string;
}