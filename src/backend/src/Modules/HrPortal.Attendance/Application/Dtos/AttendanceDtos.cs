namespace HrPortal.Attendance.Application.Dtos;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record AttendanceSessionDto(
    Guid Id,
    Guid EmployeeId,
    DateTime CheckIn,
    DateTime? CheckOut,
    double? LatitudeCheckIn,
    double? LongitudeCheckIn,
    double? LatitudeCheckOut,
    double? LongitudeCheckOut,
    double? AccuracyCheckIn,
    double? AccuracyCheckOut,
    string? Device,
    string? Browser,
    int? WorkedMinutes,
    string Status);

public sealed record CheckInRequest(
    double? Latitude = null,
    double? Longitude = null,
    double? Accuracy = null,
    string? Timezone = null,
    string? Device = null,
    string? Browser = null);

public sealed record CheckOutRequest(
    double? Latitude = null,
    double? Longitude = null,
    double? Accuracy = null,
    string? Device = null,
    string? Browser = null);

public sealed record CheckOutResponseDto(
    Guid SessionId,
    DateTime CheckIn,
    DateTime CheckOut,
    int WorkedMinutes,
    string Status);

public sealed record AttendanceDashboardDto(
    DateTime? TodayCheckIn,
    DateTime? TodayCheckOut,
    int TodayWorkedMinutes,
    AttendanceSessionDto? CurrentSession,
    int WeeklyTotalMinutes,
    int MonthlyTotalMinutes);

public sealed record GetAttendanceHistoryQuery(
    int Page = 1,
    int PageSize = 10,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? EmployeeId = null);

public sealed record GeofenceZoneDto(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    bool IsActive,
    string? Description);

public sealed record CreateGeofenceZoneRequest(
    string Name,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string? Description = null);

public sealed record UpdateGeofenceZoneRequest(
    string Name,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    bool IsActive,
    string? Description = null);

public sealed record GeofenceSettingsDto(bool GeofencingEnabled, bool AllowCheckInWithoutGps);

public sealed record UpdateGeofenceSettingsRequest(bool GeofencingEnabled, bool AllowCheckInWithoutGps);
