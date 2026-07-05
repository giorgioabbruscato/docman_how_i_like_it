using HrPortal.Projects.Application;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;
using HrPortal.Reporting.Infrastructure.Export;
using HrPortal.TimeTracking.Application;

namespace HrPortal.Reporting.Application.Generators;

internal sealed class ProjectsReportGenerator : IReportGenerator
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ITimeEntryAnalyticsProvider _timeEntryAnalyticsProvider;

    public ProjectsReportGenerator(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        ITimeEntryAnalyticsProvider timeEntryAnalyticsProvider)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _timeEntryAnalyticsProvider = timeEntryAnalyticsProvider;
    }

    public string ReportType => "projects";

    public async Task<(byte[] Content, string ContentType, string FileName)> GenerateAsync(
        ReportQueryParams query,
        ReportGenerateFilter scope,
        CancellationToken cancellationToken = default)
    {
        var from = query.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = query.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var page = await _projectRepository.GetPagedAsync(
            new GetProjectsQuery(Page: 1, PageSize: 10_000),
            cancellationToken);

        var projects = page.Items.AsEnumerable();
        if (query.ProjectId.HasValue)
            projects = projects.Where(p => p.Id == query.ProjectId);

        var spentByProject = (await _timeEntryAnalyticsProvider.GetMinutesByProjectAsync(
            from,
            to,
            query.ProjectId,
            scope.EmployeeId,
            scope.AllowedEmployeeIds,
            cancellationToken))
            .ToDictionary(r => r.Id, r => Math.Round(r.Minutes / 60m, 2));

        var headers = new[] { "Name", "Customer", "Status", "Budget Hours", "Spent Hours", "Member Count" };
        var rows = new List<IReadOnlyList<object?>>();

        foreach (var project in projects.OrderBy(p => p.Name))
        {
            var members = await _memberRepository.GetByProjectIdAsync(project.Id, cancellationToken);
            spentByProject.TryGetValue(project.Id, out var spentHours);

            rows.Add([
                project.Name,
                project.CustomerName,
                project.Status.ToString(),
                project.BudgetHours,
                spentHours,
                members.Count
            ]);
        }

        return ReportFileWriter.Write("Projects Report", "projects-report", query.Format, headers, rows);
    }
}
