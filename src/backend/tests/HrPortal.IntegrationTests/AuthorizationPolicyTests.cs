using System.Net;
using System.Net.Http.Json;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

/// <summary>
/// Verifies endpoint access decisions for the permission-based authorization model (Task 22).
/// Legacy Keycloak realm roles (Admin/HR/Manager/Employee) are mapped to fine-grained permissions via
/// <c>LegacyRoleMapper</c> + <c>SystemRoleTemplates</c> when a user has no explicit tenant membership,
/// so the role-based matrices below double as a regression check that permission grants per system role
/// reproduce (and refine) the original policy behavior. Endpoints requiring more than one permission use
/// <c>[RequireAnyPermission]</c> (OR semantics) to grant parity between tenant-scoped (HR) and
/// team/self-scoped (Manager/Employee) access.
/// </summary>
public sealed class AuthorizationPolicyTests : IntegrationTestBase
{
    private static readonly Guid MissingId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public AuthorizationPolicyTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Theory]
    [MemberData(nameof(GetEmployeesPolicyCases))]
    public async Task GetEmployees_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/employees");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AuthenticatedOnlyPolicyCases))]
    public async Task GetEmployeeById_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync($"/api/v1/employees/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminPolicyCases))]
    public async Task PostEmployees_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);
        var email = $"auth.{Guid.NewGuid():N}@demo.local";

        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Auth",
            lastName = "Test",
            email,
            hireDate = "2024-01-15"
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminNotFoundPolicyCases))]
    public async Task PutEmployees_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.PutAsJsonAsync($"/api/v1/employees/{MissingId}", new
        {
            firstName = "Auth",
            lastName = "Test",
            email = "auth.test@demo.local"
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminNotFoundPolicyCases))]
    public async Task DeleteEmployees_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.DeleteAsync($"/api/v1/employees/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminListPolicyCases))]
    public async Task GetDepartments_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/departments");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminPolicyCases))]
    public async Task PostDepartments_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);
        var code = $"D{Guid.NewGuid():N}"[..6].ToUpperInvariant();

        var response = await client.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Policy Test",
            code,
            description = "Auth policy matrix"
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AllowAnonymousPolicyCases))]
    public async Task GetTenants_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role, includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/tenants");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AllowAnonymousCreatePolicyCases))]
    public async Task PostTenants_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role, includeTenantHeader: false);
        var slug = $"t{Guid.NewGuid():N}"[..10].ToLowerInvariant();

        var response = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Policy Test Tenant",
            slug
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(GetEmployeesPolicyCases))]
    public async Task GetDocuments_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/documents");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AuthenticatedOnlyPolicyCases))]
    public async Task GetDocumentById_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync($"/api/v1/documents/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminNotFoundPolicyCases))]
    public async Task DeleteDocuments_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.DeleteAsync($"/api/v1/documents/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(GetEmployeesPolicyCases))]
    public async Task GetLeaveRequests_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/leave-requests");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AuthenticatedOnlyPolicyCases))]
    public async Task GetLeaveRequestById_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync($"/api/v1/leave-requests/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(ManagerOrAboveNotFoundPolicyCases))]
    public async Task ApproveLeaveRequests_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.PutAsync($"/api/v1/leave-requests/{MissingId}/approve", null);

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(GetEmployeesPolicyCases))]
    public async Task GetAttendance_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/attendance");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(GetEmployeesPolicyCases))]
    public async Task GetAttendanceReports_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/attendance/reports?from=2024-01-01&to=2024-01-31");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminListPolicyCases))]
    public async Task GetRoles_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/roles");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminNotFoundPolicyCases))]
    public async Task GetRoleById_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync($"/api/v1/roles/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AdminOnlyCreatePolicyCases))]
    public async Task PostRoles_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);
        var slug = $"role-{Guid.NewGuid():N}"[..14];

        var response = await client.PostAsJsonAsync("/api/v1/roles", new
        {
            slug,
            permissions = new[] { "employee.read:self" }
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AdminOnlyNotFoundPolicyCases))]
    public async Task PutRoles_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.PutAsJsonAsync($"/api/v1/roles/{MissingId}", new
        {
            permissions = new[] { "employee.read:self" }
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AdminOnlyNotFoundPolicyCases))]
    public async Task DeleteRoles_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.DeleteAsync($"/api/v1/roles/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminListPolicyCases))]
    public async Task GetMemberships_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/v1/memberships");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminBadRequestPolicyCases))]
    public async Task PostMemberships_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        // Empty roleIds fails service-level validation (400) for callers who pass the authorization
        // gate; the point of this matrix is confirming who reaches the handler at all (401/403 vs 400).
        var response = await client.PostAsJsonAsync("/api/v1/memberships", new
        {
            userId = Guid.NewGuid(),
            roleIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(HrOrAdminNotFoundPolicyCases))]
    public async Task PutMemberships_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.PutAsJsonAsync($"/api/v1/memberships/{MissingId}", new
        {
            roleIds = new[] { Guid.NewGuid() }
        });

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [MemberData(nameof(AdminOnlyNotFoundPolicyCases))]
    public async Task DeleteMemberships_PolicyMatrix_ReturnsExpectedStatus(string? role, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role);

        var response = await client.DeleteAsync($"/api/v1/memberships/{MissingId}");

        response.StatusCode.Should().Be(expectedStatus);
    }

    public static TheoryData<string?, HttpStatusCode> GetEmployeesPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.OK },
        { "hr", HttpStatusCode.OK },
        { "admin", HttpStatusCode.OK }
    };

    public static TheoryData<string?, HttpStatusCode> AuthenticatedOnlyPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.NotFound },
        { "manager", HttpStatusCode.NotFound },
        { "hr", HttpStatusCode.NotFound },
        { "admin", HttpStatusCode.NotFound }
    };

    public static TheoryData<string?, HttpStatusCode> AuthenticatedListPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.OK },
        { "manager", HttpStatusCode.OK },
        { "hr", HttpStatusCode.OK },
        { "admin", HttpStatusCode.OK }
    };

    public static TheoryData<string?, HttpStatusCode> HrOrAdminPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.Forbidden },
        { "hr", HttpStatusCode.Created },
        { "admin", HttpStatusCode.Created }
    };

    public static TheoryData<string?, HttpStatusCode> HrOrAdminNotFoundPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.Forbidden },
        { "hr", HttpStatusCode.NotFound },
        { "admin", HttpStatusCode.NotFound }
    };

    public static TheoryData<string?, HttpStatusCode> ManagerOrAboveNotFoundPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.NotFound },
        { "hr", HttpStatusCode.NotFound },
        { "admin", HttpStatusCode.NotFound }
    };

    public static TheoryData<string?, HttpStatusCode> HrOrAdminBadRequestPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.Forbidden },
        { "hr", HttpStatusCode.BadRequest },
        { "admin", HttpStatusCode.BadRequest }
    };

    public static TheoryData<string?, HttpStatusCode> HrOrAdminListPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.Forbidden },
        { "hr", HttpStatusCode.OK },
        { "admin", HttpStatusCode.OK }
    };

    public static TheoryData<string?, HttpStatusCode> AdminOnlyCreatePolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.Forbidden },
        { "hr", HttpStatusCode.Forbidden },
        { "admin", HttpStatusCode.Created }
    };

    public static TheoryData<string?, HttpStatusCode> AdminOnlyNotFoundPolicyCases() => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { "employee", HttpStatusCode.Forbidden },
        { "manager", HttpStatusCode.Forbidden },
        { "hr", HttpStatusCode.Forbidden },
        { "admin", HttpStatusCode.NotFound }
    };

    public static TheoryData<string?, HttpStatusCode> AllowAnonymousPolicyCases() => new()
    {
        { null, HttpStatusCode.OK },
        { "employee", HttpStatusCode.OK },
        { "manager", HttpStatusCode.OK },
        { "hr", HttpStatusCode.OK },
        { "admin", HttpStatusCode.OK }
    };

    public static TheoryData<string?, HttpStatusCode> AllowAnonymousCreatePolicyCases() => new()
    {
        { null, HttpStatusCode.Created },
        { "employee", HttpStatusCode.Created },
        { "manager", HttpStatusCode.Created },
        { "hr", HttpStatusCode.Created },
        { "admin", HttpStatusCode.Created }
    };
}
