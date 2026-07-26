using MediatR;

namespace WorkVault.Application.Modules.Dashboard.Queries.GetHeadcountTrend;

/// <summary>
/// Monthly joiner counts for the current year — powers the dashboard headcount chart.
/// </summary>
public record GetHeadcountTrendQuery : IRequest<IReadOnlyList<MonthlyJoinDto>>;

/// <summary>One month's joiner count.</summary>
/// <param name="Month">Short month label (e.g. "Jan").</param>
/// <param name="Count">Employees who joined that month.</param>
public record MonthlyJoinDto(string Month, int Count);
