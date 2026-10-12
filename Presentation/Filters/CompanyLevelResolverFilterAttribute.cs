
namespace Presentation.Filters;

public class CompanyLevelResolverFilterAttribute(
    ITenantResolver tenantResolver) : Attribute, IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var roleClaim = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);

        if ( roleClaim == UserRole.CompanyManager.ToString())
        {
            var companyId = context.HttpContext.User.FindFirstValue("companyId");
            var ownerId = context.HttpContext.User.FindFirstValue("ownerId");

            if (String.IsNullOrWhiteSpace(companyId))
                throw new InvalidOperationException("CompanyId is missing from the user identity.");

            tenantResolver.SetCompanyId(companyId);

            if (String.IsNullOrWhiteSpace(ownerId))
                throw new InvalidOperationException("OwnerId is missing from the user identity.");

            tenantResolver.SetOwnerId(ownerId);

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
