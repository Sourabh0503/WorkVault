/** Dashboard aggregate counts returned by `GET /api/dashboard/stats`. */
export interface DashboardStats {
  totalEmployees: number;
  activeEmployees: number;
  pendingInvites: number;
  totalDepartments: number;
}

/** One month's joiner count from `GET /api/dashboard/headcount`. */
export interface HeadcountPoint {
  month: string; // "Jan"
  count: number;
}

