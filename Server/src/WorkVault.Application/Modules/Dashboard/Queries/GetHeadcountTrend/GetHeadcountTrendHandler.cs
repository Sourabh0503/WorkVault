using System.Globalization;
using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Dashboard.Queries.GetHeadcountTrend;

/// <summary>
/// Builds the year-to-date headcount trend: one bucket per month from January through
/// the current month, filling zero for months with no joiners.
/// </summary>
public class GetHeadcountTrendHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetHeadcountTrendQuery, IReadOnlyList<MonthlyJoinDto>>
{
    public async Task<IReadOnlyList<MonthlyJoinDto>> Handle(
        GetHeadcountTrendQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var counts = await employeeRepository.GetMonthlyJoinCountsAsync(now.Year, cancellationToken);

        var trend = new List<MonthlyJoinDto>(now.Month);
        for (var month = 1; month <= now.Month; month++)
        {
            var label = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            trend.Add(new MonthlyJoinDto(label, counts.GetValueOrDefault(month, 0)));
        }

        return trend;
    }
}
