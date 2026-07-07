namespace WorkVault.SharedKernel.Models;

/// <summary>A single page of results plus the paging metadata needed to render a pager.</summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items on this page.</param>
/// <param name="TotalCount">Total number of items across all pages.</param>
/// <param name="PageNumber">The 1-based page number this result represents.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    /// <summary>Total number of pages, derived from <see cref="TotalCount"/> and <see cref="PageSize"/>.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}