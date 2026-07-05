// Mirrors PagedResult<T> (WorkVault.SharedKernel.Models).
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
