
namespace Presentation.Filters;

public class GetCompanyManagerResolverFilterAttribute(
    ITenantResolver tenantResolver) : Attribute, IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var roleClaim = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);

        if ( roleClaim == UserRole.CompanyManager.ToString()
            || roleClaim == UserRole.DepartmentManager.ToString())
        {
            var companyId = context.HttpContext.User.FindFirstValue("companyId");
            var ownerId = context.HttpContext.User.FindFirstValue("ownerId");

            tenantResolver.SetCompanyId(companyId!);

            tenantResolver.SetOwnerId(ownerId!);


        } else if (roleClaim == UserRole.Owner.ToString())
        {
            var companyId = context.HttpContext.Request.Headers["company-Id"].ToString();
            var id = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(companyId))
                throw new InvalidOperationException("CompanyId header is missing from the request.");

            tenantResolver.SetCompanyId(companyId);
            tenantResolver.SetOwnerId(id!);
        }
        else
        {
            throw new InvalidOperationException("User role is not recognized for tenant resolution.");
        }

        return next();
    }
}
