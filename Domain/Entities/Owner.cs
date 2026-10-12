
namespace Domain.Entities;


public class Owner : User
{
    
    public List<Company> Companies { get; set; } 

    private Owner() : base()
    {
        Companies = [];
    }
    

    private Owner(string firstName, string lastName, string email
        , string hashedPassword, string phoneNumber, Address address) 
        
        : base(firstName, lastName, email, hashedPassword, phoneNumber, address, UserRole.Owner)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        HashedPassword = hashedPassword;
        Address = address;
        CreatedAt = DateTime.UtcNow;
        Companies = [];
    }
    public static Owner Create(string firstName, string lastName, string email
        , string hashedPassword, string phoneNumber, Address address)
    {
        return new(firstName, lastName, email, hashedPassword, phoneNumber, address);
    }
}

public sealed record Address(string Country, string City, string Street);
