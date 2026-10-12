
namespace Presentation.Controllers.V1;

[ApiVersion("1.0")]
public class CompanyController(
    ICompanyService companyService) : AppControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Owner")]
    [TypeFilter(typeof(OwnerLevelResolverFilterAttribute))]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request
        , CancellationToken cancellationToken = default)
    {
        var result = await companyService.CreateAsync(request, cancellationToken);

        return NewResponse(result);
    }
    [HttpDelete("{companyId}")]
    [Authorize(Roles = "Owner")]
    [TypeFilter(typeof(OwnerLevelResolverFilterAttribute))]
    public async Task<IActionResult> Delete([FromRoute] string companyId
        , CancellationToken cancellationToken = default)
    {
        var result = await companyService.DeleteAsync(companyId, cancellationToken);

        return NewResponse(result);
    }

    [HttpGet("{companyId}")]
    [Authorize(Roles = "Owner")]
    [TypeFilter(typeof(OwnerLevelResolverFilterAttribute))]
    public async Task<IActionResult> GetById([FromRoute] string companyId, CancellationToken cancellationToken = default)
    {
        var result = await companyService.GetByIdAsync(companyId, cancellationToken);

        return NewResponse(result);
    }

    [HttpGet]
    [Authorize(Roles = "Owner")]
    [TypeFilter(typeof(OwnerLevelResolverFilterAttribute))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await companyService.GetAllAsync(cancellationToken);

        return NewResponse(result);
    }

    [HttpPut("{companyId}")]
    [Authorize(Roles = "Owner")]
    [TypeFilter(typeof(OwnerLevelResolverFilterAttribute))]
    public async Task<IActionResult> Update([FromRoute] string companyId, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken = default)
    {
        var result = await companyService.UpdateAsync(request, companyId, cancellationToken);

        return NewResponse(result);
    }

    [HttpGet("manager")]
    [Authorize(Roles = "Owner,CompanyManager,DepartmentManager")]
    [TypeFilter(typeof(GetCompanyManagerResolverFilterAttribute))]
    public async Task<IActionResult> GetManager(CancellationToken cts = default)
    {
        // Id is resolved from tenant resolver

        var response = await companyService.GetManagerAsync(cts);

        return NewResponse(response);
    }
    [HttpPatch("{id}/set-manager/{managerId}")]
    [Authorize(Roles = "Owner")]
    [TypeFilter(typeof(OwnerLevelResolverFilterAttribute))]
    public async Task<IActionResult> SetManager(string id,string managerId, CancellationToken cts = default)
    {

        var response = await companyService.SetManagerAsync(id,managerId, cts);

        return NewResponse(response);
    }

    [HttpPatch("{id}/unset-manager")]
    [Authorize(Roles = "Owner")]
    [TypeFilter(typeof(OwnerLevelResolverFilterAttribute))]
    public async Task<IActionResult> UnsetManager(string id, CancellationToken cts = default)
    {

        var response = await companyService.UnSetManagerAsync(id, cts);

        return NewResponse(response);
    }
}
