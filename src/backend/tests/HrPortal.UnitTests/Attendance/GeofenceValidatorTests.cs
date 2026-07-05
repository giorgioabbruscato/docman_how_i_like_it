using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;

namespace HrPortal.UnitTests.Attendance;

public sealed class GeofenceValidatorTests
{
    private readonly GeofenceValidator _validator = new();

    [Fact]
    public void CalculateDistanceMeters_ReturnsZero_ForSamePoint()
    {
        _validator.CalculateDistanceMeters(40.7128, -74.006, 40.7128, -74.006).Should().Be(0);
    }

    [Fact]
    public void IsWithinAnyZone_ReturnsTrue_WhenInsideRadius()
    {
        var zone = GeofenceZone.Create(Guid.NewGuid(), "Office", 45.0, 9.0, 500);
        _validator.IsWithinAnyZone(45.0001, 9.0001, [zone]).Should().BeTrue();
    }

    [Fact]
    public void IsWithinAnyZone_ReturnsFalse_WhenOutsideRadius()
    {
        var zone = GeofenceZone.Create(Guid.NewGuid(), "Office", 45.0, 9.0, 10);
        _validator.IsWithinAnyZone(46.0, 10.0, [zone]).Should().BeFalse();
    }
}
