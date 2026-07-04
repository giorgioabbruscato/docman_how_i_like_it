using HrPortal.Employees.Domain;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.UnitTests.Employees;

public sealed class EmployeeDomainTests
{
    [Fact]
    public void Create_NormalizesEmailToLowercase()
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            "Mario",
            "Rossi",
            "MARIO.ROSSI@Demo.Local",
            new DateOnly(2024, 1, 15));

        employee.Email.Should().Be("mario.rossi@demo.local");
        employee.IsActive.Should().BeTrue();
        employee.FullName.Should().Be("Mario Rossi");
    }

    [Fact]
    public void Update_NormalizesEmail()
    {
        var employee = Employee.Create(
            Guid.NewGuid(), "Mario", "Rossi", "mario@demo.local", new DateOnly(2024, 1, 15));

        employee.Update("Luigi", "Verdi", "LUIGI@Demo.Local", "Dev", null, Guid.NewGuid());

        employee.FirstName.Should().Be("Luigi");
        employee.Email.Should().Be("luigi@demo.local");
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var employee = Employee.Create(
            Guid.NewGuid(), "Mario", "Rossi", "mario@demo.local", new DateOnly(2024, 1, 15));

        employee.Deactivate(Guid.NewGuid());

        employee.IsActive.Should().BeFalse();
    }
}
