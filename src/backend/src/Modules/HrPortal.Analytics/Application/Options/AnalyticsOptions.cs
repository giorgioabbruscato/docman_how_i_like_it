namespace HrPortal.Analytics.Application.Options;

public sealed class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    public int DailyStandardMinutes { get; set; } = 480;

    public TimeOnly LateCheckInTime { get; set; } = new(9, 0);

    public DayOfWeek[] Workdays { get; set; } =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];
}
