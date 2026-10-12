using Application.ResultPattern;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Services;

public interface IEmployeeService
{
    Task<Response<string>> CreateAsync(CreateEmployeeRequest request, CancellationToken cts = default);
    Task<Response<EmployeeResponse>> GetByIdAsync(string employeeId, CancellationToken cts = default);
    Task<Response<List<EmployeeResponse>>> GetAllByDepartmentIdAsync(CancellationToken cts = default);
    Task<Response<List<EmployeeResponse>>> GetAllByManagerIdAsync(string managerId, CancellationToken cts = default);
    Task<Response<List<EmployeeResponse>>> GetAllByCompanyIdAsync(CancellationToken cts = default);
    Task<Response<string>> UpdateAsync(UpdateEmployeeRequest request, string employeeId, CancellationToken cts = default);
    Task<Response<string>> DeleteAsync(string employeeId, string callerRole, CancellationToken cts = default);
    Task<Response<string>> SetManagerAsync(SetEmployeeManagerRequest request, string callerId, string callerRole, CancellationToken cts = default);
    Task<Response<string>> UnsetManagerAsync(string employeeId, string callerId, string callerRole, CancellationToken cts = default);
}




public static class EmployeeRoleConversion
{
    public static UserRole ConvertToUserRole(EmployeeRole employeeRole)
    {
        return employeeRole switch
        {
            EmployeeRole.CompanyManager => UserRole.CompanyManager,
            EmployeeRole.DepartmentManager => UserRole.DepartmentManager,
            EmployeeRole.Employee => UserRole.Employee,
            _ => UserRole.Employee,
        };
    }
}

public enum EmployeeRole
{
    CompanyManager,
    DepartmentManager,
    Employee
}

public sealed record CreateEmployeeRequest
{
    [Required]
    public string FirstName { get; init; } = default!;
    [Required]
    public string LastName { get; init; } = default!;
    [Required]
    public EmployeeRole Role { get; init; }
    [Required]
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
    public string? ManagerId { get; init; }
}

public sealed record UpdateEmployeeRequest
{
    [Required]
    public string FirstName { get; init; } = default!;
    [Required]
    public string LastName { get; init; } = default!;
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;
    [Required]
    public string PhoneNumber { get; init; } = default!;
    [Required]
    public Address Address { get; init; } = default!;
}

public sealed record SetEmployeeManagerRequest(
    string EmployeeId,
    string ManagerId);


public sealed record EmployeeResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Role,
    Address Address,
    string DepartmentId,
    string DpartmentName,
    string? ManagerName,
    DateTime CreatedAt
);

public static class EmployeeMappings
{
    public static EmployeeResponse ToResponse(this Employee employee)
    {
        return new(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.PhoneNumber,
            employee.Role.ToString(),
            employee.Address,
            employee.DepartmentId,
            employee.Department.Name,
            employee.Manager?.GetFullName(),
            employee.CreatedAt
        );
    }
}

