namespace Infrustructure.Abstractions;

public sealed class TenantResolver : ITenantResolver
{
    public string CompanyId { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public string DepartmentId { get; private set; } = string.Empty;

    public string GetCompanyId() => CompanyId;

    public void SetCompanyId(string companyId) => CompanyId = companyId;

    public string GetOwnerId() => OwnerId;
    public void SetOwnerId(string ownerId) => OwnerId = ownerId;
    public string GetDepartmentId() => DepartmentId;
    public void SetDepartmentId(string departmentId) => DepartmentId = departmentId;
}