/**
 * Access roles assignable to an employee via the create/edit form.
 *
 * The GUIDs mirror the seeded system roles on the backend (WorkVault.SharedKernel
 * SystemRoles). CompanyAdmin/SuperAdmin are intentionally omitted — CompanyAdmin is
 * the founder established at registration, and the API rejects any other role.
 */
export interface RoleOption {
  id: string;
  label: string;
}

export const HR_ROLE_ID = '33333333-3333-3333-3333-333333333333';
export const MANAGER_ROLE_ID = '44444444-4444-4444-4444-444444444444';
export const EMPLOYEE_ROLE_ID = '55555555-5555-5555-5555-555555555555';

// Order shown in the dropdown; Employee is the default selection.
export const ASSIGNABLE_ROLES: RoleOption[] = [
  { id: EMPLOYEE_ROLE_ID, label: 'Employee' },
  { id: MANAGER_ROLE_ID, label: 'Manager' },
  { id: HR_ROLE_ID, label: 'HR' },
];
