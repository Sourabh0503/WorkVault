// Matches DepartmentDto from GetDepartmentsHandler.cs
export interface Department {
  id: string;
  name: string;
  description: string | null;
  parentDepartmentId: string | null;
  parentDepartmentName: string | null;
  headEmployeeId: string | null;
  headEmployeeName: string | null;
  memberCount: number;
}

// POST /api/departments — request body
export interface CreateDepartmentRequest {
  name: string;
  description?: string;
  parentDepartmentId?: string;
  headEmployeeId?: string;
}

// POST /api/departments — response
export interface CreateDepartmentResult {
  id: string;
  name: string;
}

// PUT /api/departments/{id} — request body
export interface UpdateDepartmentRequest {
  id: string;
  name: string;
  description?: string;
  parentDepartmentId?: string;
  headEmployeeId?: string;
}