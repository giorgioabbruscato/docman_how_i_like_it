using System.Xml.Linq;

namespace HrPortal.UnitTests.Architecture;

public sealed class ModuleDependencyTests
{
    private static readonly string ModulesPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Modules"));

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> AllowedModuleDependencies =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            ["HrPortal.Departments"] = new HashSet<string>(StringComparer.Ordinal),
            ["HrPortal.Employees"] = new HashSet<string>(StringComparer.Ordinal) { "HrPortal.Departments" },
            ["HrPortal.Leave"] = new HashSet<string>(StringComparer.Ordinal) { "HrPortal.Employees", "HrPortal.Calendar" },
            ["HrPortal.Calendar"] = new HashSet<string>(StringComparer.Ordinal) { "HrPortal.Employees", "HrPortal.Departments" },
            ["HrPortal.Attendance"] = new HashSet<string>(StringComparer.Ordinal) { "HrPortal.Employees" },
            ["HrPortal.Documents"] = new HashSet<string>(StringComparer.Ordinal) { "HrPortal.Employees" },
            ["HrPortal.Projects"] = new HashSet<string>(StringComparer.Ordinal) { "HrPortal.Employees" },
            ["HrPortal.Tasks"] = new HashSet<string>(StringComparer.Ordinal) { "HrPortal.Employees", "HrPortal.Projects" },
            ["HrPortal.TimeTracking"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "HrPortal.Employees", "HrPortal.Projects", "HrPortal.Tasks"
            },
            ["HrPortal.Analytics"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "HrPortal.Departments",
                "HrPortal.Employees",
                "HrPortal.TimeTracking",
                "HrPortal.Attendance",
                "HrPortal.Leave",
                "HrPortal.Projects",
                "HrPortal.Tasks"
            },
            ["HrPortal.Reporting"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "HrPortal.Departments",
                "HrPortal.Employees",
                "HrPortal.TimeTracking",
                "HrPortal.Attendance",
                "HrPortal.Projects"
            }
        };

    [Fact]
    public void ModuleProjects_HaveOnlyAllowedModuleReferences()
    {
        var actualDependencies = ReadModuleDependencies();

        actualDependencies.Keys.Should().BeEquivalentTo(AllowedModuleDependencies.Keys);

        foreach (var (module, dependencies) in actualDependencies)
        {
            var allowed = AllowedModuleDependencies[module];
            dependencies.Should().BeSubsetOf(allowed,
                because: $"{module} must only depend on explicitly allowed modules");
        }
    }

    [Fact]
    public void ModuleProjects_HaveNoCircularDependencies()
    {
        var dependencies = ReadModuleDependencies();

        foreach (var module in dependencies.Keys)
        {
            HasCycle(module, dependencies, new HashSet<string>(StringComparer.Ordinal)).Should().BeFalse(
                because: $"{module} must not participate in a circular module dependency");
        }
    }

    private static Dictionary<string, HashSet<string>> ReadModuleDependencies()
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var projectFile in Directory.EnumerateFiles(ModulesPath, "HrPortal.*.csproj", SearchOption.AllDirectories))
        {
            var moduleName = Path.GetFileNameWithoutExtension(projectFile);
            var moduleReferences = new HashSet<string>(StringComparer.Ordinal);

            var document = XDocument.Load(projectFile);
            foreach (var reference in document.Descendants("ProjectReference"))
            {
                var include = reference.Attribute("Include")?.Value;
                if (include is null)
                    continue;

                var referencedProject = Path.GetFileNameWithoutExtension(include);
                if (referencedProject.StartsWith("HrPortal.", StringComparison.Ordinal)
                    && AllowedModuleDependencies.ContainsKey(referencedProject))
                {
                    moduleReferences.Add(referencedProject);
                }
            }

            result[moduleName] = moduleReferences;
        }

        return result;
    }

    private static bool HasCycle(
        string module,
        IReadOnlyDictionary<string, HashSet<string>> dependencies,
        ISet<string> visiting)
    {
        if (!visiting.Add(module))
            return true;

        if (!dependencies.TryGetValue(module, out var moduleDependencies))
            return false;

        foreach (var dependency in moduleDependencies)
        {
            if (HasCycle(dependency, dependencies, visiting))
                return true;
        }

        visiting.Remove(module);
        return false;
    }
}
