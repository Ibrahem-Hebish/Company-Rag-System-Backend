
namespace Domain.Entities;

public class User 
{
    public string Id { get; set; }
    public string FirstName { get; protected set; } = string.Empty;
    public string LastName { get; protected set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Address Address { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; protected set; }
    public List<Token> Tokens { get; private set; }
    protected User() { }
    protected User(string firstName, string lastName, string email, string hashedPassword,
        string phoneNumber, Address address,UserRole userType)
    {
        Id = Guid.NewGuid().ToString();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        HashedPassword = hashedPassword;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = DateTime.UtcNow;
        Role = userType;
        Tokens = [];
    }
    public static User Create(string firstName, string lastName, string email, string hashedPassword, string phoneNumber, Address address, UserRole userType)
    {
        return new(firstName, lastName, email, hashedPassword, phoneNumber, address, userType);
    }

    public void ChangePassword(string newHashedPassword)
    {
        HashedPassword = newHashedPassword;
        UpdatedAt = DateTime.UtcNow;
    }
}


public enum UserRole
{
    Owner,
    CompanyManager,
    DepartmentManager,
    Employee
}
