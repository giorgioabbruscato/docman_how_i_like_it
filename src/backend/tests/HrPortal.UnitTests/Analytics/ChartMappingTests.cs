using FluentAssertions;
using HrPortal.Analytics.Application.Dtos;

namespace HrPortal.UnitTests.Analytics;

public sealed class ChartMappingTests
{
    [Fact]
    public void ToBarChart_ReturnsEmptyArrays_WhenNoRows()
    {
        var chart = ChartMapping.ToBarChart([], "Hours");

        chart.Labels.Should().BeEmpty();
        chart.Datasets.Should().BeEmpty();
    }

    [Fact]
    public void ToBarChart_OrdersByHoursDescending()
    {
        var chart = ChartMapping.ToBarChart(
        [
            new NamedHoursRow("Beta", Guid.NewGuid(), 10m),
            new NamedHoursRow("Alpha", Guid.NewGuid(), 20m)
        ],
        "Hours");

        chart.Labels.Should().Equal("Alpha", "Beta");
        chart.Datasets.Should().ContainSingle();
        chart.Datasets[0].Label.Should().Be("Hours");
        chart.Datasets[0].Data.Should().Equal(20m, 10m);
    }

    [Fact]
    public void ToLineChartFromMonths_FormatsLabelsAndData()
    {
        var chart = ChartMapping.ToLineChartFromMonths(
        [
            new MonthHoursRow(2025, 1, 40m),
            new MonthHoursRow(2025, 2, 55m)
        ],
        "Hours");

        chart.Labels.Should().Equal("Jan 2025", "Feb 2025");
        chart.Datasets[0].Data.Should().Equal(40m, 55m);
    }

    [Fact]
    public void ToBudgetConsumptionChart_ProducesUsedAndRemainingDatasets()
    {
        var chart = ChartMapping.ToBudgetConsumptionChart(
        [
            new BudgetUsageDto(Guid.NewGuid(), "Project A", 100m, 60m, 1000m, 600m)
        ]);

        chart.Labels.Should().Equal("Project A");
        chart.Datasets.Should().HaveCount(2);
        chart.Datasets[0].Label.Should().Be("Used");
        chart.Datasets[0].Data.Should().Equal(60m);
        chart.Datasets[1].Label.Should().Be("Remaining");
        chart.Datasets[1].Data.Should().Equal(40m);
    }

    [Fact]
    public void Empty_NeverReturnsNullCollections()
    {
        ChartMapping.Empty.Labels.Should().NotBeNull();
        ChartMapping.Empty.Datasets.Should().NotBeNull();
    }
}
