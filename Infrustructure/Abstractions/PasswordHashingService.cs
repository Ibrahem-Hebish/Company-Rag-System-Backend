using Application.Abstractions;

namespace Infrustructure.Abstractions;

public class PasswordHashingHandler : IPasswordHashingHandler
{
    public string HashPasswordBCrypt(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPasswordBCrypt(string password, string hashedPassword) => BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
