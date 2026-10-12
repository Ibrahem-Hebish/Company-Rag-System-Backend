using Infrustructure.Services;

namespace Presentation.Controllers.V1;

[ApiVersion("1.0")]
public class DepartmentController(IDepartmentService departmentService) : AppControllerBase
{
    [HttpPost()]
    [Authorize(Roles = "Owner,CompanyManager")]
    [TypeFilter(typeof(CompanyLevelResolverFilterAttribute))]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {

        var result = await departmentService.CreateAsync(request, cancellationToken);
       
        return NewResponse(result);
    }

    [HttpDelete("{departmentId}")]
    [Authorize(Roles = "Owner,CompanyManager")]
    [TypeFilter(typeof(CompanyLevelResolverFilterAttribute))]
    public async Task<IActionResult> Delete([FromRoute] string departmentId, CancellationToken cancellationToken = default)
    {
       
        var result = await departmentService.DeleteAsync(departmentId, cancellationToken);
        
        return NewResponse(result);
    }

    [HttpGet("{departmentId}")]
    [Authorize(Roles = "Owner,CompanyManager")]
    [TypeFilter(typeof(CompanyLevelResolverFilterAttribute))]
    public async Task<IActionResult> GetById([FromRoute] string departmentId, CancellationToken cancellationToken = default)
    {

        var result = await departmentService.GetByIdAsync(departmentId, cancellationToken);
        
        return NewResponse(result);
    }

    [HttpGet]
    [Authorize(Roles = "Owner,CompanyManager")]
    [TypeFilter(typeof(CompanyLevelResolverFilterAttribute))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        // Company id is resolved from tenant resolver

        var result = await departmentService.GetAllAsync(cancellationToken);

        return NewResponse(result);
    }

    [HttpPut("{departmentId}")]
    [Authorize(Roles = "Owner,CompanyManager")]
    [TypeFilter(typeof(CompanyLevelResolverFilterAttribute))]
    public async Task<IActionResult> Update([FromRoute] string departmentId, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
       
        var result = await departmentService.UpdateAsync(request, departmentId, cancellationToken);
        
        return NewResponse(result);
    }

    [HttpGet("manager")]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]
    [TypeFilter(typeof(GetDepartmentManagerResolverFilterAttribute))]
    public async Task<IActionResult> GetManager(CancellationToken cts = default)
    {
        // Id is resolved from tenant resolver

        var response = await departmentService.GetManagerAsync(cts);

        return NewResponse(response);
    }

    [HttpPatch("{id}/set-manager/{managerId}")]
    [Authorize(Roles = "Owner,CompanyManager")]
    [TypeFilter(typeof(CompanyLevelResolverFilterAttribute))]
    public async Task<IActionResult> SetManager(string id, string managerId, CancellationToken cts = default)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var callerRole = User.FindFirstValue(ClaimTypes.Role);

        var response = await departmentService.SetManagerAsync(id,managerId, callerId!, callerRole!, cts);

        return NewResponse(response);
    }
    [HttpPatch("{id}/unset-manager")]
    [Authorize(Roles = "Owner,CompanyManager")]
    [TypeFilter(typeof(CompanyLevelResolverFilterAttribute))]
    public async Task<IActionResult> UnsetManager(string id, CancellationToken cts = default)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var callerRole = User.FindFirstValue(ClaimTypes.Role);

        var response = await departmentService.UnsetManagerAsync(id, callerId!, callerRole!, cts);

        return NewResponse(response);
    }
    
}