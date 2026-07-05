using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;

namespace HrPortal.Attendance.Application;

internal static class AttendanceSessionMapping
{
    public static AttendanceSessionDto ToDto(AttendanceSession session) =>
        new(
            session.Id,
            session.EmployeeId,
            session.CheckIn,
            session.CheckOut,
            session.LatitudeCheckIn,
            session.LongitudeCheckIn,
            session.LatitudeCheckOut,
            session.LongitudeCheckOut,
            session.AccuracyCheckIn,
            session.AccuracyCheckOut,
            session.Device,
            session.Browser,
            session.WorkedMinutes,
            session.Status.ToString());
}
