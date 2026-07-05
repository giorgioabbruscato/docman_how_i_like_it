using System.Xml.Linq;
using FluentAssertions;

namespace HrPortal.UnitTests.Architecture;

public sealed class PlatformDependencyTests
{
    private static readonly string PlatformPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Platform"));

    [Fact]
    public void AccessControl_HasNoBusinessModuleReferences()
    {
        var projectFile = Path.Combine(PlatformPath, "HrPortal.AccessControl", "HrPortal.AccessControl.csproj");
        File.Exists(projectFile).Should().BeTrue();

        var document = XDocument.Load(projectFile);
        var references = document.Descendants("ProjectReference")
            .Select(r => Path.GetFileNameWithoutExtension(r.Attribute("Include")?.Value ?? string.Empty))
            .Where(name => name.StartsWith("HrPortal.", StringComparison.Ordinal))
            .ToList();

        references.Should().NotContain("HrPortal.Employees");
        references.Should().NotContain("HrPortal.Departments");
        references.Should().NotContain("HrPortal.Leave");
        references.Should().NotContain("HrPortal.Attendance");
        references.Should().NotContain("HrPortal.Documents");
    }
}
