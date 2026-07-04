using Microsoft.OpenApi.Any;

namespace HrPortal.Api.Infrastructure.OpenApi;

internal static class OpenApiExamples
{
    private static readonly OpenApiString SampleUuid = new("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private static readonly OpenApiString SampleDate = new("2024-01-15");
    private static readonly OpenApiString SampleDateTime = new("2025-07-04T10:00:00Z");

    public static readonly OpenApiObject ProblemDetailsNotFound = new()
    {
        ["type"] = new OpenApiString("https://httpstatuses.com/404"),
        ["title"] = new OpenApiString("Not found"),
        ["status"] = new OpenApiInteger(404),
        ["detail"] = new OpenApiString("Employee with key '...' was not found."),
        ["errorCode"] = new OpenApiString("NOT_FOUND")
    };

    public static readonly OpenApiObject ProblemDetailsBadRequest = new()
    {
        ["type"] = new OpenApiString("https://httpstatuses.com/400"),
        ["title"] = new OpenApiString("Bad request"),
        ["status"] = new OpenApiInteger(400),
        ["detail"] = new OpenApiString("Validation failed."),
        ["errorCode"] = new OpenApiString("VALIDATION_ERROR")
    };

    public static readonly OpenApiArray TenantList = new()
    {
        new OpenApiObject
        {
            ["id"] = SampleUuid,
            ["name"] = new OpenApiString("Demo Company"),
            ["slug"] = new OpenApiString("demo"),
            ["isActive"] = new OpenApiBoolean(true)
        }
    };

    public static readonly OpenApiObject TenantCreated = new()
    {
        ["id"] = SampleUuid,
        ["name"] = new OpenApiString("Acme Corp"),
        ["slug"] = new OpenApiString("acme")
    };

    public static readonly OpenApiObject CreateTenantRequest = new()
    {
        ["name"] = new OpenApiString("Acme Corp"),
        ["slug"] = new OpenApiString("acme")
    };

    public static readonly OpenApiArray EmployeeList = new()
    {
        new OpenApiObject
        {
            ["id"] = SampleUuid,
            ["firstName"] = new OpenApiString("Mario"),
            ["lastName"] = new OpenApiString("Rossi"),
            ["email"] = new OpenApiString("mario.rossi@demo.local"),
            ["jobTitle"] = new OpenApiString("Developer"),
            ["departmentId"] = SampleUuid,
            ["hireDate"] = SampleDate,
            ["isActive"] = new OpenApiBoolean(true)
        }
    };

    public static readonly OpenApiObject Employee = new()
    {
        ["id"] = SampleUuid,
        ["firstName"] = new OpenApiString("Mario"),
        ["lastName"] = new OpenApiString("Rossi"),
        ["email"] = new OpenApiString("mario.rossi@demo.local"),
        ["jobTitle"] = new OpenApiString("Developer"),
        ["departmentId"] = SampleUuid,
        ["hireDate"] = SampleDate,
        ["isActive"] = new OpenApiBoolean(true)
    };

    public static readonly OpenApiObject CreateEmployeeRequest = new()
    {
        ["firstName"] = new OpenApiString("Mario"),
        ["lastName"] = new OpenApiString("Rossi"),
        ["email"] = new OpenApiString("mario.rossi@demo.local"),
        ["hireDate"] = SampleDate,
        ["jobTitle"] = new OpenApiString("Developer"),
        ["departmentId"] = SampleUuid
    };

    public static readonly OpenApiObject UpdateEmployeeRequest = new()
    {
        ["firstName"] = new OpenApiString("Mario"),
        ["lastName"] = new OpenApiString("Rossi"),
        ["email"] = new OpenApiString("mario.rossi@demo.local"),
        ["jobTitle"] = new OpenApiString("Senior Developer"),
        ["departmentId"] = SampleUuid
    };

    public static readonly OpenApiArray DepartmentList = new()
    {
        new OpenApiObject
        {
            ["id"] = SampleUuid,
            ["name"] = new OpenApiString("Engineering"),
            ["code"] = new OpenApiString("ENG"),
            ["description"] = new OpenApiString("Software development"),
            ["parentDepartmentId"] = new OpenApiNull(),
            ["isActive"] = new OpenApiBoolean(true)
        }
    };

    public static readonly OpenApiObject Department = new()
    {
        ["id"] = SampleUuid,
        ["name"] = new OpenApiString("Engineering"),
        ["code"] = new OpenApiString("ENG"),
        ["description"] = new OpenApiString("Software development"),
        ["parentDepartmentId"] = new OpenApiNull(),
        ["isActive"] = new OpenApiBoolean(true)
    };

    public static readonly OpenApiObject CreateDepartmentRequest = new()
    {
        ["name"] = new OpenApiString("Engineering"),
        ["code"] = new OpenApiString("ENG"),
        ["description"] = new OpenApiString("Software development"),
        ["parentDepartmentId"] = new OpenApiNull()
    };

    public static readonly OpenApiObject LeaveRequest = new()
    {
        ["id"] = SampleUuid,
        ["employeeId"] = SampleUuid,
        ["startDate"] = new OpenApiString("2025-07-01"),
        ["endDate"] = new OpenApiString("2025-07-05"),
        ["type"] = new OpenApiString("Annual"),
        ["status"] = new OpenApiString("Pending"),
        ["reason"] = new OpenApiString("Summer holiday"),
        ["approvedBy"] = new OpenApiNull(),
        ["approvedAt"] = new OpenApiNull()
    };

    public static readonly OpenApiObject CreateLeaveRequest = new()
    {
        ["employeeId"] = SampleUuid,
        ["startDate"] = new OpenApiString("2025-07-01"),
        ["endDate"] = new OpenApiString("2025-07-05"),
        ["type"] = new OpenApiString("Annual"),
        ["reason"] = new OpenApiString("Summer holiday")
    };

    public static readonly OpenApiObject RejectLeaveRequest = new()
    {
        ["reason"] = new OpenApiString("Insufficient coverage")
    };

    public static readonly OpenApiArray AttendanceList = new()
    {
        new OpenApiObject
        {
            ["id"] = SampleUuid,
            ["employeeId"] = SampleUuid,
            ["date"] = new OpenApiString("2025-07-04"),
            ["checkIn"] = new OpenApiString("09:00:00"),
            ["checkOut"] = new OpenApiNull(),
            ["status"] = new OpenApiString("Present"),
            ["notes"] = new OpenApiNull()
        }
    };

    public static readonly OpenApiObject AttendanceRecord = new()
    {
        ["id"] = SampleUuid,
        ["employeeId"] = SampleUuid,
        ["date"] = new OpenApiString("2025-07-04"),
        ["checkIn"] = new OpenApiString("09:00:00"),
        ["checkOut"] = new OpenApiNull(),
        ["status"] = new OpenApiString("Present"),
        ["notes"] = new OpenApiNull()
    };

    public static readonly OpenApiObject CheckInRequest = new()
    {
        ["employeeId"] = SampleUuid,
        ["date"] = new OpenApiString("2025-07-04"),
        ["time"] = new OpenApiString("09:00:00")
    };

    public static readonly OpenApiObject AttendanceReport = new()
    {
        ["from"] = new OpenApiString("2025-07-01"),
        ["to"] = new OpenApiString("2025-07-31"),
        ["totalRecords"] = new OpenApiInteger(22),
        ["presentCount"] = new OpenApiInteger(18),
        ["absentCount"] = new OpenApiInteger(2),
        ["lateCount"] = new OpenApiInteger(1),
        ["halfDayCount"] = new OpenApiInteger(1),
        ["remoteCount"] = new OpenApiInteger(0)
    };

    public static readonly OpenApiArray DocumentList = new()
    {
        new OpenApiObject
        {
            ["id"] = SampleUuid,
            ["employeeId"] = SampleUuid,
            ["fileName"] = new OpenApiString("contract.pdf"),
            ["contentType"] = new OpenApiString("application/pdf"),
            ["sizeBytes"] = new OpenApiLong(102400),
            ["category"] = new OpenApiString("Contract"),
            ["uploadedAt"] = SampleDateTime,
            ["uploadedBy"] = SampleUuid
        }
    };

    public static readonly OpenApiArray LeaveRequestList = new()
    {
        new OpenApiObject
        {
            ["id"] = SampleUuid,
            ["employeeId"] = SampleUuid,
            ["startDate"] = new OpenApiString("2025-07-01"),
            ["endDate"] = new OpenApiString("2025-07-05"),
            ["type"] = new OpenApiString("Annual"),
            ["status"] = new OpenApiString("Pending"),
            ["reason"] = new OpenApiString("Summer holiday"),
            ["approvedBy"] = new OpenApiNull(),
            ["approvedAt"] = new OpenApiNull()
        }
    };

    public static readonly OpenApiObject Document = new()
    {
        ["id"] = SampleUuid,
        ["employeeId"] = SampleUuid,
        ["fileName"] = new OpenApiString("contract.pdf"),
        ["contentType"] = new OpenApiString("application/pdf"),
        ["sizeBytes"] = new OpenApiLong(102400),
        ["category"] = new OpenApiString("Contract"),
        ["uploadedAt"] = SampleDateTime,
        ["uploadedBy"] = SampleUuid
    };
}
