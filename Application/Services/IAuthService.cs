using Application.ResultPattern;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Application.Services;

public interface IAuthService
{
    Task<Response<string>> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<Response<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Response<RefreshTokenResponse>> RefreshTokenAsync(string tokenId, string refreshToken, CancellationToken cancellationToken = default);
    Task<Response<string>> LogoutAsync(string tokenId, CancellationToken cancellationToken = default);
    //Task<Response<string>> GetCodeToResetPassword(ObtainCodeToResetPassswordRequest request, IMemoryCache cache, CancellationToken cancellationToken = default);
    //Task<Response<string>> VerifyResetPasswordCode(VerifyResetCode request, IMemoryCache cache, CancellationToken cancellationToken = default);
    //Task<Response<string>> ResetPassword(ResetPasswordRequest request, IMemoryCache cache, CancellationToken cancellationToken = default);
    Task<Response<string>> ChangePasswordAsync(ChangePasswordRequest request, string userId, CancellationToken cancellationToken = default);
}


public sealed record ObtainCodeToResetPassswordRequest(string Email);
public sealed record VerifyResetCode
{
    [EmailAddress]
    public string Email { get; init; } = default!;
    [Required]
    public string Code { get; init; } = default!;
}
public sealed record ResetPasswordRequest
{
    [EmailAddress]
    public string Email { get; init; } = default!;
    [Required]
    public string Token { get; init; } = default!;
    [Required]
    [PasswordValidation]
    public string NewPassword { get; init; } = default!;
    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = default!;
}


public sealed record ChangePasswordRequest
{
    [Required]
    [PasswordValidation]
    public string CurrentPassword { get; init; } = default!;
    [Required]
    [PasswordValidation]
    public string NewPassword { get; init; } = default!;
    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = default!;
}

public sealed record RegisterUserRequest
{
    [Required]
    public string FirstName { get; init; } = default!;
    [Required]
    public string LastName { get; init; } = default!;
    [EmailAddress]
    public string Email { get; init; } = default!;
    [Required]
    [PasswordValidation]
    public string Password { get; init; } = default!;
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = default!;
    [Required]
    public string PhoneNumber { get; init; } = default!;
    [Required]
    public Address Address { get; init; } = default!;
}

public sealed record LoginRequest(
    [EmailAddress]
    string Email,
    [Required]
    [PasswordValidation]
    string Password);


public sealed record LoginResponse(
    string UserId,
    string Role);
public sealed record RefreshTokenResponse(
    string UserId,
    string Role);

public sealed class PasswordValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
            return new ValidationResult("Password is required.");

        string password = value.ToString()!;

        if (password.Length < 8)
            return new ValidationResult(
                "Password must be at least 8 characters long.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return new ValidationResult(
                "Password must contain at least one uppercase letter.");

        if (!Regex.IsMatch(password, @"[a-z]"))
            return new ValidationResult(
                "Password must contain at least one lowercase letter.");

        if (!Regex.IsMatch(password, @"\d"))
            return new ValidationResult(
                "Password must contain at least one digit.");

        if (!Regex.IsMatch(password, @"[\W_]"))
            return new ValidationResult(
                "Password must contain at least one special character.");

        return ValidationResult.Success;
    }
}
