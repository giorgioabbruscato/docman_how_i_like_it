namespace HrPortal.Analytics.Application.Dtos;

public sealed record ChartDatasetDto(string Label, IReadOnlyList<decimal> Data);

public sealed record ChartResponseDto(
    IReadOnlyList<string> Labels,
    IReadOnlyList<ChartDatasetDto> Datasets);

public static class ChartMapping
{
    public static ChartResponseDto Empty { get; } = new([], []);

    public static ChartResponseDto ToBarChart(
        IReadOnlyList<NamedHoursRow> rows,
        string datasetLabel)
    {
        if (rows.Count == 0)
            return Empty;

        var ordered = rows.OrderByDescending(r => r.Hours).ToList();
        return new ChartResponseDto(
            ordered.Select(r => r.Label).ToList(),
            [new ChartDatasetDto(datasetLabel, ordered.Select(r => r.Hours).ToList())]);
    }

    public static ChartResponseDto ToLineChartFromMonths(
        IReadOnlyList<MonthHoursRow> rows,
        string datasetLabel)
    {
        if (rows.Count == 0)
            return Empty;

        var ordered = rows.OrderBy(r => r.Year).ThenBy(r => r.Month).ToList();
        return new ChartResponseDto(
            ordered.Select(r => new DateOnly(r.Year, r.Month, 1).ToString("MMM yyyy")).ToList(),
            [new ChartDatasetDto(datasetLabel, ordered.Select(r => r.Hours).ToList())]);
    }

    public static ChartResponseDto ToLineChartFromDates(
        IReadOnlyList<DateHoursRow> rows,
        string datasetLabel)
    {
        if (rows.Count == 0)
            return Empty;

        var ordered = rows.OrderBy(r => r.Date).ToList();
        return new ChartResponseDto(
            ordered.Select(r => r.Date.ToString("yyyy-MM-dd")).ToList(),
            [new ChartDatasetDto(datasetLabel, ordered.Select(r => r.Hours).ToList())]);
    }

    public static ChartResponseDto ToBudgetConsumptionChart(IReadOnlyList<BudgetUsageDto> rows)
    {
        if (rows.Count == 0)
            return Empty;

        var ordered = rows.OrderBy(r => r.ProjectName).ToList();
        var used = new List<decimal>();
        var remaining = new List<decimal>();

        foreach (var row in ordered)
        {
            var budget = row.BudgetHours ?? 0m;
            var spent = row.SpentHours;
            used.Add(spent);
            remaining.Add(Math.Max(0m, budget - spent));
        }

        return new ChartResponseDto(
            ordered.Select(r => r.ProjectName).ToList(),
            [
                new ChartDatasetDto("Used", used),
                new ChartDatasetDto("Remaining", remaining)
            ]);
    }
}
