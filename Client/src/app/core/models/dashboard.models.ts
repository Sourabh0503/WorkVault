/** Dashboard aggregate counts returned by `GET /api/dashboard/stats`. */
export interface DashboardStats {
  totalEmployees: number;
  activeEmployees: number;
  pendingInvites: number;
  totalDepartments: number;
}

