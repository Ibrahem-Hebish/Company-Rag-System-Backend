using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Application.Abstractions;

public interface ITokenHandler
{
    Task<TokenValidationResult> ValidateAccessToken(string accessToken);
    string CreateAccessToken(ClaimsIdentity claims);
}
