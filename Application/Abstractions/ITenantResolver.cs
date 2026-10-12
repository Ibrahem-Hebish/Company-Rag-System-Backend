namespace Application.Abstractions;

public interface ITenantResolver
{
    public string CompanyId { get; }
    public string OwnerId { get; }
    public string DepartmentId { get; }

    string GetCompanyId();
    public void SetCompanyId(string companyId);

    string GetOwnerId();
    public void SetOwnerId(string ownerId);
    string GetDepartmentId();
    public void SetDepartmentId(string departmentId);
}