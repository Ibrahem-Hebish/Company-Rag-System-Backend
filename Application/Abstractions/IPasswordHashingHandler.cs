namespace Application.Abstractions;

public interface IPasswordHashingHandler
{
    string HashPasswordBCrypt(string password);
    bool VerifyPasswordBCrypt(string password, string hashedPassword);
}