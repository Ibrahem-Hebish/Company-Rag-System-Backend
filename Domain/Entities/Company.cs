
namespace Domain.Entities;

public sealed class Company
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } 
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string OwnerId { get; init; }
    public Owner Owner { get; init; }
    public string? ManagerId { get; private set; }
    public Employee? Manager { get; private set; }
    public List<Department> Departments { get; set; }
    public List<Employee> Employees { get; set; }
    public List<Document> Documents { get; set; }

    private Company() { }

    private Company(string name, string email, string phoneNumber,Address address
        , string ownerId, string? managerId)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        OwnerId = ownerId;
        ManagerId = managerId;
        Departments = [];
        Employees = [];
        Documents = [];
    }

    public static Company Create(string name, string email, string phoneNumber, Address address
        , string ownerId, string? managerId)
    {
        return new(name, email, phoneNumber, address, ownerId, managerId);
    }

    public void Update(string name, string email, string phoneNumber, Address address, string? managerId)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        ManagerId = managerId;
    }

    public void SetManager(string id)
    {
        ManagerId = id;
    }
    public void UnSetManager()
    {
        ManagerId = null;
    }
    public void ChabgeManager(string id)
    {
        ManagerId = id;
    }
}
