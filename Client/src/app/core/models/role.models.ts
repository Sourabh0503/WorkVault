/**
 * Access roles assignable to an employee via the create/edit form.
 *
 * The GUIDs mirror the seeded system roles on the backend (WorkVault.SharedKernel
 * SystemRoles). SuperAdmin is platform-level and never assignable here. CompanyAdmin is
 * included so a tenant can have more than one admin (and can't be permanently locked out).
 */
export interface RoleOption {
  id: string;
  label: string;
}

export const COMPANY_ADMIN_ROLE_ID = '22222222-2222-2222-2222-222222222222';
export const HR_ROLE_ID = '33333333-3333-3333-3333-333333333333';
export const MANAGER_ROLE_ID = '44444444-4444-4444-4444-444444444444';
export const EMPLOYEE_ROLE_ID = '55555555-5555-5555-5555-555555555555';

// Order shown in the dropdown; Employee is the default selection.
export const ASSIGNABLE_ROLES: RoleOption[] = [
  { id: EMPLOYEE_ROLE_ID, label: 'Employee' },
  { id: MANAGER_ROLE_ID, label: 'Manager' },
  { id: HR_ROLE_ID, label: 'HR' },
  { id: COMPANY_ADMIN_ROLE_ID, label: 'Company Admin' },
];
