using HrPortal.SharedKernel.Results;

namespace HrPortal.Reporting.Application;

public sealed class ReportGeneratorFactory
{
    private readonly IReadOnlyDictionary<string, IReportGenerator> _generators;

    public ReportGeneratorFactory(IEnumerable<IReportGenerator> generators) =>
        _generators = generators.ToDictionary(g => g.ReportType, StringComparer.OrdinalIgnoreCase);

    public Result<IReportGenerator> Resolve(string type)
    {
        if (_generators.TryGetValue(type, out var generator))
            return Result.Success(generator);

        return Result.Failure<IReportGenerator>($"Unknown report type '{type}'.", "NOT_FOUND");
    }
}
