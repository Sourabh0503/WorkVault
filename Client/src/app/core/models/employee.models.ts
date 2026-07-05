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
}

// POST /api/employees — response
export interface CreateEmployeeResult {
  employeeId: string;
  employeeCode: string;
  inviteLink: string;
}