
namespace Domain.Entities;

public class Employee : User
{
    public string DepartmentId { get; private set; }
    public Department Department { get; set; }
    public string? ManagerId { get; private set; }
    public Employee? Manager { get; private set; }
    public string CompanyId { get; init; }
    public Company Company { get; init; }
    public List<Document> UploadedDocuments { get; private set; }
    public List<Document> ModifiedDocuments { get; private set; }


    private Employee() : base()
    {
        UploadedDocuments = [];
        ModifiedDocuments = [];
    }

    private Employee(string firstName, string lastName,UserRole role, string email
        , string hashedPassword, string phoneNumber, Address address, string departmentId,
        string companyId,string? managerId)
        : base(firstName, lastName, email, hashedPassword, phoneNumber, address, role)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        HashedPassword = hashedPassword;
        Address = address;
        CreatedAt = DateTime.UtcNow;
        DepartmentId = departmentId;
        CompanyId = companyId;
        ManagerId = managerId;
        UploadedDocuments = [];
        ModifiedDocuments = [];
    }
    public static Employee Create(string firstName, string lastName, UserRole role, string email
        , string hashedPassword, string phoneNumber, Address address, string departmentId, string companyId,
        string? managerId)
    {
        return new(firstName, lastName,role, email, hashedPassword, phoneNumber, address, departmentId, companyId, managerId);
    }

    public string? GetFullName() => $"{FirstName} {LastName}";

    public void Update(string firstName, string lastName, string email, string phoneNumber, Address address)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
    }

    public void SetManager(string id)
    {
        ManagerId = id;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnSetManager()
    {
        ManagerId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanSetManager(Employee manager, string callerId, string callerRole)
    {
        if (this.Role == UserRole.Employee)
        {
            if (callerRole == UserRole.DepartmentManager.ToString()
                && manager.Id == callerId)
                return true;

            else if (callerRole == UserRole.CompanyManager.ToString()
                && this.Company.ManagerId == callerId)
                return true;

            else if (callerRole == UserRole.Owner.ToString()
                && this.Company.OwnerId == callerId)
                return true;
        }
        else if (this.Role == UserRole.DepartmentManager)
        {
            if (callerRole == UserRole.CompanyManager.ToString()
                && this.Company.ManagerId == callerId)
                return true;

            else if (callerRole == UserRole.Owner.ToString()
                && this.Company.OwnerId == callerId)
                return true;
        }
        else
            return false;

        return false;
    }

    public bool CanUnsetManager(string callerId, string callerRole)
    {

        if(this.Role == UserRole.Employee)
        {
            if (callerRole == UserRole.DepartmentManager.ToString()
            && this.Department.ManagerId == callerId)
                return true;

            else if (callerRole == UserRole.CompanyManager.ToString()
                && this.Company.ManagerId == callerId)
                return true;

            else if (callerRole == UserRole.Owner.ToString()
                && this.Company.OwnerId == callerId)
                return true;

        }else if(this.Role == UserRole.DepartmentManager)
        {
            if (callerRole == UserRole.CompanyManager.ToString()
                && this.Company.ManagerId == callerId)
                return true;

            else if (callerRole == UserRole.Owner.ToString()
                && this.Company.OwnerId == callerId)
                return true;
        }
        

        return false;

    }


    public bool IsValidManager(Employee manager)
    {
        if(this.Role == UserRole.Employee)
        {
            if (this.Department.ManagerId == manager.Id)
                return true;
        }
        else if(this.Role == UserRole.DepartmentManager)
        {
            if (this.Company.ManagerId == manager.Id)
                return true;
        }

        return false;
    }
        
}
