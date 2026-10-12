
namespace Domain.Entities;

public class Token
{
    public string Id { get; init; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; init; }
    public DateTime AccessTokenExpirationDate { get; set; }
    public DateTime RefreshTokenExpirationDate { get; init; }
    public string UserId { get; init; }
    public User User { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; private set; }

    private Token() { }

    private Token(string accessToken, string refreshToken, DateTime accessTokenExpirationDate, DateTime refreshTokenExpirationDate, string userId)
    {
        Id = Guid.NewGuid().ToString();
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpirationDate = accessTokenExpirationDate;
        RefreshTokenExpirationDate = refreshTokenExpirationDate;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public static Token Create(string accessToken, string refreshToken, DateTime accessTokenExpirationDate, DateTime refreshTokenExpirationDate, string userId)
    {
        return new(accessToken, refreshToken, accessTokenExpirationDate, refreshTokenExpirationDate, userId);
    }

    public void MarkAsInactive()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanRefresh() => IsActive && RefreshTokenExpirationDate > DateTime.UtcNow 
        && AccessTokenExpirationDate < DateTime.UtcNow;

    public void RefreshAccessToken(string newAccessToken, DateTime accessTokenExpiration)
    {
        AccessToken = newAccessToken;
        AccessTokenExpirationDate = accessTokenExpiration;
        UpdatedAt = DateTime.UtcNow;
    }
}