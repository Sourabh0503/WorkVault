/**
 * Designation types for the designations feature — the list/read model plus the
 * create/update request DTOs. Each mirrors its backend counterpart (noted inline).
 */

// Matches DesignationListDto from GetDesignationsHandler.cs
export interface Designation {
  id: string;
  title: string;
  level: number;
  departmentId: string;
  departmentName: string;
}

// POST /api/designations — request body (CreateDesignationCommand)
export interface CreateDesignationRequest {
  title: string;
  level: number;
  departmentId: string;
}

// POST /api/designations — response (CreateDesignationResult)
export interface CreateDesignationResult {
  id: string;
  title: string;
}

// PUT /api/designations/{id} — request body (UpdateDesignationCommand)
export interface UpdateDesignationRequest {
  id: string;
  title: string;
  level: number;
  departmentId: string;
}
