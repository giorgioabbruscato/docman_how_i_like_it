using HrPortal.Employees.Application;
using HrPortal.Employees.Application.Dtos;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Employee CRUD operations.</summary>
[ApiController]
[Route("api/v1/employees")]
[Tags("Employees")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService) =>
        _employeeService = employeeService;

    /// <summary>List all employees.</summary>
    /// <remarks>Auth: ManagerOrAbove</remarks>
    [HttpGet]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get employee by ID.</summary>
    /// <remarks>Auth: Authenticated</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a new employee.</summary>
    /// <remarks>Auth: HrOrAdmin</remarks>
    [HttpPost]
    [Authorize(Policy = Policies.HrOrAdmin)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Update an employee.</summary>
    /// <remarks>Auth: HrOrAdmin</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.HrOrAdmin)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Deactivate an employee (soft delete).</summary>
    /// <remarks>Auth: HrOrAdmin</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.HrOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.DeactivateAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = result.Error
            }),
            "CONFLICT" => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = result.Error
            }),
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = result.Error
            })
        };
}
