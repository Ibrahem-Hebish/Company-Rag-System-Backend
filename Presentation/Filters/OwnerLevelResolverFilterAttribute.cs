
namespace Presentation.Filters;

public class OwnerLevelResolverFilterAttribute(
    ITenantResolver tenantResolver) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var roleClaim = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);

        if (roleClaim != UserRole.Owner.ToString())
            throw new InvalidOperationException("Role is ino allowed.");

        var id = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        tenantResolver.SetOwnerId(id!);

        await next();
    }
}
