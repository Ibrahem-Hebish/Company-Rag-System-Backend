using Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrustructure.Abstractions;

public class TokenHandler(
    IOptions<JwtOptions> jwtOptions,
    IConfiguration configuration)
    : ITokenHandler

{

    public async Task<TokenValidationResult> ValidateAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var signingkey = configuration["jwtsigningkey"];
        var issuer = jwtOptions.Value.Issuer;
        var audience = jwtOptions.Value.Audience;

        if (string.IsNullOrWhiteSpace(signingkey))
            throw new InvalidOperationException("Signing key is missing from configuration.");

        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = issuer ?? "jjjjjj",
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        signingkey!))
        };

        tokenValidationParameters.ValidIssuer = issuer;
        tokenValidationParameters.ValidAudience = audience;

        var result = await tokenHandler.ValidateTokenAsync(
            accessToken, tokenValidationParameters);

        return result;
    }
    public string CreateAccessToken(ClaimsIdentity claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var signingkey = configuration["jwtsigningkey"];

        if (string.IsNullOrWhiteSpace(signingkey))
            throw new InvalidOperationException("Signing key is missing from configuration.");

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Issuer = jwtOptions.Value.Issuer,

            Audience = jwtOptions.Value.Audience,

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        signingkey!)),
                SecurityAlgorithms.HmacSha256),

            Subject = claims,

            Expires = DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenLifeTime)

        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        var accessToken = tokenHandler.WriteToken(securityToken);

        return accessToken;
    }

}
