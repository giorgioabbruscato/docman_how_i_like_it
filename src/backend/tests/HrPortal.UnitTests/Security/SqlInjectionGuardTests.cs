namespace HrPortal.UnitTests.Security;

public sealed class SqlInjectionGuardTests
{
    [Fact]
    public void BackendSource_DoesNotUseRawSqlQueries()
    {
        var backendRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src"));

        var csFiles = Directory.GetFiles(backendRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        csFiles.Should().NotBeEmpty();

        var forbiddenTokens = new[]
        {
            "FromSqlRaw(",
            "FromSqlInterpolated(",
            "ExecuteSqlRaw(",
            "ExecuteSqlRawAsync(",
            "ExecuteSqlInterpolated(",
            "ExecuteSqlInterpolatedAsync("
        };

        var violations = csFiles
            .SelectMany(file => forbiddenTokens
                .Where(token => File.ReadAllText(file).Contains(token, StringComparison.Ordinal))
                .Select(token => $"{file}: {token}"))
            .ToList();

        violations.Should().BeEmpty("EF Core parameterized queries must be used instead of raw SQL");
    }
}
