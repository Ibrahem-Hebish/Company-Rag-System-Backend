
namespace Presentation.Controllers.V1;

[ApiVersion("1.0")]
public class AuthController(IAuthService authService) : AppControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request
        , CancellationToken cancellationToken = default)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        return NewResponse(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request
        , CancellationToken cancellationToken = default)
    {
        var result = await authService.RegisterUserAsync(request, cancellationToken);

        return NewResponse(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken = default)
    {
        var tokenId = Request.Cookies["token-id"]?.ToString();

        var refreshToken = Request.Cookies["refresh-token"]?.ToString();

        if(string.IsNullOrWhiteSpace(tokenId) || string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new { message = "Token ID and Refresh Token are required." });

        var result = await authService.RefreshTokenAsync(tokenId, refreshToken, cancellationToken);

        return NewResponse(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var tokenId = Request.Cookies["token-id"]?.ToString();

        if (string.IsNullOrWhiteSpace(tokenId))
            return BadRequest(new { message = "Token ID is required." });
        
        var result = await authService.LogoutAsync(tokenId, cancellationToken);

        return NewResponse(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request
        , CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "User ID is required." });

        var result = await authService.ChangePasswordAsync(request, userId, cancellationToken);
        return NewResponse(result);
    }
}
