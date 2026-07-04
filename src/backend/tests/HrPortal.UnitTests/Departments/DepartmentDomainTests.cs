using HrPortal.Departments.Domain;

namespace HrPortal.UnitTests.Departments;

public sealed class DepartmentDomainTests
{
    [Fact]
    public void Create_UppercasesCode()
    {
        var department = Department.Create(Guid.NewGuid(), "Engineering", "eng");

        department.Code.Should().Be("ENG");
        department.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_UppercasesCode()
    {
        var department = Department.Create(Guid.NewGuid(), "Engineering", "ENG");

        department.Update("Dev", "ops", null, null, Guid.NewGuid());

        department.Code.Should().Be("OPS");
        department.Name.Should().Be("Dev");
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var department = Department.Create(Guid.NewGuid(), "Engineering", "ENG");

        department.Deactivate(Guid.NewGuid());

        department.IsActive.Should().BeFalse();
    }
}
