namespace Infrustructure;

public class JwtOptions
{
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int AccessTokenLifeTime { get; init; }
    public int RefreshTokenLifeTime { get; init; }
    public string SigningKey { get; init; } = "";
}
