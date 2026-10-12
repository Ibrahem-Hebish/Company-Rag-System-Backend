namespace Presentation.Controllers.V1;


[ApiVersion("1.0")]
[TypeFilter(typeof(DepartmentLevelResolverFilterAttribute))]
public class EmployeeController(IEmployeeService employeeService) : AppControllerBase
{
    [HttpGet()]
    [Authorize(Roles = "Owner,CompanyManager")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await employeeService.GetAllByCompanyIdAsync(cancellationToken);
        return NewResponse(result);
    }
    [HttpGet("departments")]
    [Authorize(Roles = "Employee,Owner,CompanyManager,DepartmentManager")]
    public async Task<IActionResult> GetAllByDepartmentId(CancellationToken cancellationToken = default)
    {
        // departmentId is resolved from tenant resolver

        var result = await employeeService.GetAllByDepartmentIdAsync(cancellationToken);

        return NewResponse(result);
    }

    [HttpGet("managers/{managerId}")]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]
    public async Task<IActionResult> GetAllByManagerId(string managerId, CancellationToken cancellationToken = default)
    {
        var result = await employeeService.GetAllByManagerIdAsync(managerId, cancellationToken);

        return NewResponse(result);
    }

    [HttpGet("{employeeId}")]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]
    public async Task<IActionResult> GetById(string employeeId, CancellationToken cancellationToken = default)
    {
        var result = await employeeService.GetByIdAsync(employeeId, cancellationToken);

        return NewResponse(result);
    }
    [HttpPost()]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var result = await employeeService.CreateAsync(request, cancellationToken);
        return NewResponse(result);
    }
    [HttpPut("{employeeId}")]
    [Authorize(Roles = "Owner,Employee,CompanyManager,DepartmentManager")]
    public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await employeeService.UpdateAsync(request, employeeId!, cancellationToken);

        return NewResponse(result);
    }
    [HttpDelete("{employeeId}")]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]
    public async Task<IActionResult> Delete(string employeeId, CancellationToken cancellationToken = default)
    {
        var callerRole = User.FindFirstValue(ClaimTypes.Role);

        var result = await employeeService.DeleteAsync(employeeId, callerRole!, cancellationToken);

        return NewResponse(result);
    }

    [HttpPatch("set-manager")]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]

    public async Task<IActionResult> SetManager(SetEmployeeManagerRequest request , CancellationToken cts = default)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var callerRole = User.FindFirstValue(ClaimTypes.Role);

        var response = await employeeService.SetManagerAsync(request, callerId!, callerRole!, cts);

        return NewResponse(response);
    }

    [HttpPatch("unset-manager/{id}")]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]

    public async Task<IActionResult> UnsetManager(string id , CancellationToken cts = default)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var callerRole = User.FindFirstValue(ClaimTypes.Role);

        var response = await employeeService.UnsetManagerAsync(id, callerId! , callerRole!, cts);

        return NewResponse(response);
    }
}
