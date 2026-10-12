namespace Presentation.Filters;

public class DepartmentLevelResolverFilterAttribute(
    ITenantResolver tenantResolver) : Attribute, IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var roleClaim = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);

        if (roleClaim == UserRole.Employee.ToString()
            || roleClaim == UserRole.DepartmentManager.ToString())
        {
            var companyId = context.HttpContext.User.FindFirstValue("companyId");
            var ownerId = context.HttpContext.User.FindFirstValue("ownerId");
            var departmentId = context.HttpContext.User.FindFirstValue("departmentId");


            tenantResolver.SetCompanyId(companyId!);
            tenantResolver.SetDepartmentId(departmentId!);
            tenantResolver.SetOwnerId(ownerId!);


        }else if(roleClaim == UserRole.CompanyManager.ToString())
        {
            var ownerId = context.HttpContext.User.FindFirstValue("ownerId");

            var companyId = context.HttpContext.Request.Headers["company-Id"].ToString();
            var departmentId = context.HttpContext.Request.Headers["department-Id"].ToString();


            if (String.IsNullOrWhiteSpace(companyId))
                throw new InvalidOperationException("CompanyId header is missing from the request.");

            if (String.IsNullOrWhiteSpace(departmentId))
                throw new InvalidOperationException("DepartmentId header is missing from the request.");

            tenantResolver.SetCompanyId(companyId);
            tenantResolver.SetOwnerId(ownerId!);
            tenantResolver.SetDepartmentId(departmentId);
        }
        else if (roleClaim == UserRole.Owner.ToString())
        {
            var companyId = context.HttpContext.Request.Headers["company-Id"].ToString();
            var departmentId = context.HttpContext.Request.Headers["department-Id"].ToString();

            var id = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(companyId))
                throw new InvalidOperationException("CompanyId header is missing from the request.");

            if (String.IsNullOrWhiteSpace(departmentId))
                throw new InvalidOperationException("DepartmentId header is missing from the request.");

            tenantResolver.SetCompanyId(companyId);
            tenantResolver.SetOwnerId(id!);
            tenantResolver.SetDepartmentId(departmentId);
        }
        else
        {
            throw new InvalidOperationException("User role is not recognized for tenant resolution.");
        }

        return next();
    }
}
