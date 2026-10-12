
namespace Domain.Entities;

public class Department
{
    public string Id { get; init; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; init; } 
    public DateTime? UpdatedAt { get; private set; }
    public string CompanyId { get; init; }
    public Company Company { get; init; }
    public string? ManagerId { get; private set; }
    public Employee? Manager { get; private set; }
    public List<Employee> Employees { get; private set; }
    public List<Document> Documents { get; private set; }

    private Department() { }

    private Department(string name, string companyId)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        CompanyId = companyId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Department Create(string name, string companyId)
    {
        return new(name, companyId);
    }

    public void Update(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetManager(string managerId)
    {
        ManagerId = managerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetManager()
    {
        ManagerId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanSetManager(string callerId, string callerRole)
    {
        if (callerRole == UserRole.Owner.ToString() && this.Company.OwnerId == callerId) return true;
        else if (callerRole == UserRole.CompanyManager.ToString() && this.Company.ManagerId == callerId) return true;

        return false;
    }

    public bool CanUnsetManager(string id, string role) => CanSetManager(id, role);

    public bool IsValidManager(Employee manager)
    {
        if (manager.DepartmentId == this.Id
            && manager.Role == UserRole.DepartmentManager
            && this.ManagerId is null)
            return true;

        return false;
    }
}
