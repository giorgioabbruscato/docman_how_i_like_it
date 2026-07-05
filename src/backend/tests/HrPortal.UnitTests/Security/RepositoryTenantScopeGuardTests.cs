using System.Text.RegularExpressions;

namespace HrPortal.UnitTests.Security;

public sealed class RepositoryTenantScopeGuardTests
{
    private static readonly string BackendRoot = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "..", "src"));

    [Fact]
    public void Repositories_ApplyTenantScopeOnReadQueries()
    {
        var repositoryFiles = Directory.GetFiles(BackendRoot, "*Repository.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.EndsWith("TenantRepository.cs", StringComparison.Ordinal))
            .ToList();

        repositoryFiles.Should().NotBeEmpty();

        var violations = new List<string>();

        foreach (var file in repositoryFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("UserProfileRepository", StringComparison.Ordinal))
                continue;

            foreach (var method in ExtractMethods(content))
            {
                if (!method.Body.Contains(".Set<", StringComparison.Ordinal))
                    continue;

                if (IsWriteOnlyMethod(method.Body))
                    continue;

                if (method.Name.Equals("SlugExistsForTenantAsync", StringComparison.Ordinal))
                    continue;

                if (!method.Body.Contains("ApplyTenantScope", StringComparison.Ordinal))
                    violations.Add($"{file}: {method.Name} uses Set<> without ApplyTenantScope");
            }
        }

        violations.Should().BeEmpty("all repository read queries must call ApplyTenantScope");
    }

    [Fact]
    public void ProductionCode_DoesNotUseIgnoreQueryFiltersExceptAllowlisted()
    {
        var csFiles = Directory.GetFiles(BackendRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        const string allowlistedMethod = "SlugExistsForTenantAsync";
        const string allowlistedFileName = "AccessControlRepositories.cs";

        var violations = new List<string>();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("IgnoreQueryFilters", StringComparison.Ordinal))
                continue;

            if (file.EndsWith(allowlistedFileName, StringComparison.Ordinal))
            {
                var methodsWithIgnore = ExtractMethods(content)
                    .Where(m => m.Body.Contains("IgnoreQueryFilters", StringComparison.Ordinal))
                    .ToList();

                if (methodsWithIgnore.Count == 1 &&
                    methodsWithIgnore[0].Name == allowlistedMethod)
                {
                    continue;
                }
            }

            violations.Add($"{file}: IgnoreQueryFilters");
        }

        violations.Should().BeEmpty(
            "IgnoreQueryFilters is only allowed in SlugExistsForTenantAsync with explicit tenantId filter");
    }

    private static bool IsWriteOnlyMethod(string body)
    {
        if (!body.Contains(".Set<", StringComparison.Ordinal))
            return false;

        var readIndicators = new[]
        {
            "FirstOrDefaultAsync",
            "SingleOrDefaultAsync",
            "ToListAsync",
            "AnyAsync",
            "CountAsync",
            "FirstAsync",
            "SingleAsync"
        };

        return !readIndicators.Any(indicator => body.Contains(indicator, StringComparison.Ordinal));
    }

    private static IEnumerable<(string Name, string Body)> ExtractMethods(string content)
    {
        var pattern = new Regex(
            @"(?:public|private|internal)\s+(?:async\s+)?(?:Task<[^>]+>|Task|void|\w+)\s+(\w+)\s*\([^)]*\)\s*(?:=>[^;{]+;|\{)",
            RegexOptions.Singleline);

        foreach (Match match in pattern.Matches(content))
        {
            var name = match.Groups[1].Value;
            var start = match.Index;
            var body = ExtractMethodBody(content, start);
            yield return (name, body);
        }
    }

    private static string ExtractMethodBody(string content, int startIndex)
    {
        if (startIndex >= content.Length)
            return string.Empty;

        var arrowIndex = content.IndexOf("=>", startIndex, StringComparison.Ordinal);
        var braceIndex = content.IndexOf('{', startIndex);

        if (arrowIndex >= 0 && (braceIndex < 0 || arrowIndex < braceIndex))
        {
            var end = content.IndexOf(';', arrowIndex);
            return end < 0 ? content[startIndex..] : content[startIndex..(end + 1)];
        }

        if (braceIndex < 0)
            return string.Empty;

        var depth = 0;
        for (var i = braceIndex; i < content.Length; i++)
        {
            if (content[i] == '{')
                depth++;
            else if (content[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return content[startIndex..(i + 1)];
            }
        }

        return content[startIndex..];
    }
}
