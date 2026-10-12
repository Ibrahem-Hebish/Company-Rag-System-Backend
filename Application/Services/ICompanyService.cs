using Application.ResultPattern;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Services;

public interface ICompanyService
{
    Task<Response<string>> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default);
    Task<Response<CompanyResponse>> GetByIdAsync(string companyId, CancellationToken cancellationToken = default);
    Task<Response<List<CompanyResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Response<string>> UpdateAsync(UpdateCompanyRequest request, string companyId, CancellationToken cancellationToken = default);
    Task<Response<string>> DeleteAsync(string companyId, CancellationToken cancellationToken = default);
    Task<Response<string>> SetManagerAsync(string id, string managerId, CancellationToken cts = default);
    Task<Response<string>> UnSetManagerAsync(string id, CancellationToken cts = default);
    Task<Response<EmployeeResponse>> GetManagerAsync(CancellationToken cts = default);
}

public sealed record CreateCompanyRequest
{
    [Required]
    public string Name { get; init; } = default!;
    [Required]
    public Address Address { get; init; } = default!;
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;
    [Required]
    public string PhoneNumber { get; init; } = default!;
    public string? ManagerId { get; init; }
}


public sealed record UpdateCompanyRequest
{
    [Required]
    public string Name { get; init; } = default!;
    [Required]
    public Address Address { get; init; } = default!;
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;
    [Required]
    public string PhoneNumber { get; init; } = default!;
    public string? ManagerId { get; init; }
}


public sealed record CompanyResponse(
    string Id,
    string Name,
    Address Address,
    string Email,
    string PhoneNumber,
    string? ManagerName
);

public static class CompanyMappings
{
    public static CompanyResponse ToResponse(this Company company)
    {
        return new(
            company.Id,
            company.Name,
            company.Address,
            company.Email,
            company.PhoneNumber,
            company.Manager?.GetFullName()
        );
    }
}


public sealed record CompanyIsDeletedEvent(string CompanyId,List<string> DepartmentIds,
    List<string> EmployeesIds, List<string> FilePaths);