namespace Infrustructure.Services;

public class AuthService(
    AppDbContext dbContext,
    IPasswordHashingHandler passwordHashingHandler,
    ITokenHandler tokenHandler,
    IOptions<JwtOptions> jwtConfig,
    IHttpContextAccessor httpContextAccessor)
    : ResponseHandler, IAuthService
{
    public async Task<Response<string>> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await dbContext.Users
                                          .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (existingUser is not null)
            return BadRequest<string>("Email is already registered.");

        var hashedPassword = passwordHashingHandler.HashPasswordBCrypt(request.Password);

        Owner owner = Owner.Create(request.FirstName, request.LastName, request.Email, hashedPassword,
            request.PhoneNumber, request.Address);

        await dbContext.Owners.AddAsync(owner, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Created("Owner registered successfully.");
    }
    public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
                                  .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user is null)
            return BadRequest<LoginResponse>("Invalid Credintials.");

        var isCorrectPassword = passwordHashingHandler.VerifyPasswordBCrypt(request.Password, user.HashedPassword);

        if (!isCorrectPassword)
            return BadRequest<LoginResponse>("Invalid Credintials.");

        try
        {
            var claimsIdentity = await GetUserClaims(user);

            var accessToken = tokenHandler.CreateAccessToken(claimsIdentity);

            var refreshToken = Guid.NewGuid().ToString();

            var token = Token.Create(accessToken, refreshToken,
                DateTime.UtcNow.AddMinutes(jwtConfig.Value.AccessTokenLifeTime),
                DateTime.UtcNow.AddDays(jwtConfig.Value.RefreshTokenLifeTime),
                user.Id);

            await dbContext.Tokens.AddAsync(token, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            SetCookies(token.Id, accessToken, refreshToken);

            return Success(new LoginResponse(
                    user.Id,
                    user.Role.ToString()));

        }
        catch (Exception ex)
        {
            // Log the exception(ex) here if needed

            return BadRequest<LoginResponse>("Invalid Credintials.");
        }
    }
    public async Task<Response<RefreshTokenResponse>> RefreshTokenAsync(string tokenId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userToken = await dbContext.Tokens
                                       .Include(x => x.User)
                                       .FirstOrDefaultAsync(x => x.Id == tokenId, cancellationToken);

        if (userToken is null)
            return BadRequest<RefreshTokenResponse>("Invalid Token.");

        if (!userToken.CanRefresh())
            return BadRequest<RefreshTokenResponse>("Token is not valid for refresh.");

        if (userToken.RefreshToken != refreshToken)
            return BadRequest<RefreshTokenResponse>("Invalid Refresh Token.");

        var claimsIdentity = await GetUserClaims(userToken.User);

        var newAccessToken = tokenHandler.CreateAccessToken(claimsIdentity);

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtConfig.Value.AccessTokenLifeTime);

        userToken.RefreshAccessToken(newAccessToken, accessTokenExpiration);

        await dbContext.SaveChangesAsync(cancellationToken);

        SetCookies(userToken.Id, newAccessToken, userToken.RefreshToken);

        return Success(new RefreshTokenResponse(
            userToken.UserId,
            userToken.User.Role.ToString()));
    }
    //public async Task<Response<string>> GetCodeToResetPassword(ObtainCodeToResetPassswordRequest request, [FromServices] IMemoryCache cache, CancellationToken cancellationToken = default)
    //{
    //    var user = await dbContext.Users
    //                              .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

    //    if (user is null)
    //        return NotFound<string>("User not found.");

    //    var code = RandomNumberGenerator
    //                                    .GetInt32(100000, 1000000)
    //                                    .ToString();

    //    var cacheKey = $"reset-password-code-{user.Id}";

    //    var hashedCode = passwordHashingHandler.HashPasswordBCrypt(code);

    //    await cache.SetStringAsync(cacheKey, hashedCode, new DistributedCacheEntryOptions
    //    {
    //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    //    }, cancellationToken);

    //    await publisher.Publish(
    //        new CodeCreatedForResetPasswordEvent(request.Email, code)
    //        , cancellationToken);

    //    return Success<string>("Code is being sent to your email. Please check it.");
    //}
    //public async Task<Response<string>> VerifyResetPasswordCode(VerifyResetCode request, [FromServices] IMemoryCache cache, CancellationToken cancellationToken = default)
    //{
    //    var user = await dbContext.Users
    //                              .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

    //    if (user is null)
    //        return NotFound<string>("User not found.");

    //    var cacheKey = $"reset-password-code-{user.Id.Value}";

    //    var cachedCode = await cache.GetStringAsync(cacheKey, cancellationToken);

    //    if (cachedCode is null)
    //        return BadRequest<string>("Code has expired or is invalid.");

    //    var hashedRequestCode = passwordHashingHandler.VerifyPasswordBCrypt(request.Code, cachedCode);

    //    if (!hashedRequestCode)
    //        return BadRequest<string>("Invalid code.");

    //    await cache.RemoveAsync(cacheKey, cancellationToken);

    //    var cachedTokenKey = $"reset-password-token-{user.Id.Value}";

    //    var resetToken = Convert.ToBase64String(
    //                            RandomNumberGenerator.GetBytes(32));

    //    var hashedResetToken = passwordHashingHandler.HashPasswordBCrypt(resetToken);

    //    await cache.SetStringAsync(cachedTokenKey, hashedResetToken, new DistributedCacheEntryOptions
    //    {
    //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
    //    }, cancellationToken);

    //    return Success(resetToken, "Valid code. You have 3 minutes to reset your password.");
    //}
    //public async Task<Response<string>> ResetPassword(ResetPasswordRequest request, [FromServices] IMemoryCache cache, CancellationToken cancellationToken = default)
    //{
    //    var user = await dbContext.Users
    //                              .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

    //    if (user is null)
    //        return NotFound<string>("User not found.");

    //    var cacheTokenKey = $"reset-password-token-{user.Id}";

    //    var cachedToken = await cache.GetStringAsync(cacheTokenKey, cancellationToken);

    //    var isValidToken = cachedToken is not null && passwordHashingHandler.VerifyPasswordBCrypt(request.Token, cachedToken);

    //    if (!isValidToken)
    //        return BadRequest<string>("Invalid or expired token.");

    //    user.ChangePassword(passwordHashingHandler.HashPasswordBCrypt(request.NewPassword));

    //    await dbContext.SaveChangesAsync(cancellationToken);

    //    await cache.RemoveAsync(cacheTokenKey, cancellationToken);

    //    return Success<string>("Password has been reset successfully.");
    //}
    public async Task<Response<string>> LogoutAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        var userToken = await dbContext.Tokens
                                       .FindAsync(tokenId, cancellationToken);
        if (userToken is null)
            return NotFound<string>("Token not found.");

        userToken.MarkAsInactive();

        await dbContext.SaveChangesAsync(cancellationToken);

        RemoveCookies();

        return Success<string>("Logged out successfully.");
    }
    public async Task<Response<string>> ChangePasswordAsync(ChangePasswordRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
                                  .FindAsync(userId, cancellationToken);

        if (user is null)
            return NotFound<string>("User not found.");

        var isCorrectPassword = passwordHashingHandler.VerifyPasswordBCrypt(request.CurrentPassword, user.HashedPassword);

        if (!isCorrectPassword)
            return BadRequest<string>("Current password is incorrect.");

        var newHashedPassword = passwordHashingHandler.HashPasswordBCrypt(request.NewPassword);

        user.ChangePassword(newHashedPassword);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Success<string>("Password changed successfully.");
    }

    private void SetCookies(string id, string accessToken, string refreshToken)
    {
        httpContextAccessor.HttpContext!.Response.Cookies.Append("token-Id", id,
            GetCookieOptions(DateTime.UtcNow.AddDays(jwtConfig.Value.RefreshTokenLifeTime)));

        httpContextAccessor.HttpContext.Response.Cookies.Append("access-Token", accessToken,
            GetCookieOptions(DateTime.UtcNow.AddMinutes(jwtConfig.Value.AccessTokenLifeTime)));

        httpContextAccessor.HttpContext.Response.Cookies.Append("refresh-Token", refreshToken,
            GetCookieOptions(DateTime.UtcNow.AddDays(jwtConfig.Value.RefreshTokenLifeTime)));
    }
    private void RemoveCookies()
    {
        httpContextAccessor.HttpContext!.Response.Cookies.Delete("token-Id");
        httpContextAccessor.HttpContext.Response.Cookies.Delete("access-Token");
        httpContextAccessor.HttpContext.Response.Cookies.Delete("refresh-Token");
    }
    private static CookieOptions GetCookieOptions(DateTime expiresAt)
    {
        return new()
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt

        };
    }
    private async Task<ClaimsIdentity> GetUserClaims(User user)
    {
        List<Claim> claims = [];

        claims.Add(new(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claims.Add(new(ClaimTypes.Role, user.Role.ToString()));

        if (user.Role == UserRole.Employee 
            || user.Role == UserRole.CompanyManager
            || user.Role == UserRole.DepartmentManager)
        {
            // fetch the claims for the employee from the database

            var result = await dbContext.Employees
                                              .IgnoreQueryFilters()
                                              .Where(e => e.Id == user.Id)
                                              .Select(e => new
                                              {
                                                  e.DepartmentId,
                                                  e.CompanyId,
                                                  e.Department.Company.OwnerId
                                              })
                                              .FirstOrDefaultAsync();

            claims.Add(new("departmentId", result!.DepartmentId));
            claims.Add(new("companyId", result!.CompanyId));
            claims.Add(new("ownerId", result!.OwnerId));
        }

        var claimsIdentity = new ClaimsIdentity(claims);

        return claimsIdentity;

    }
}
