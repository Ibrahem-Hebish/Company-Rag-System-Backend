using Application.ResultPattern;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Services;

public interface IDepartmentService
{
    Task<Response<string>> CreateAsync(CreateDepartmentRequest request, CancellationToken cts = default);
    Task<Response<DepartmentResponse>> GetByIdAsync(string id, CancellationToken cts = default);
    Task<Response<List<DepartmentResponse>>> GetAllAsync(CancellationToken cts = default);
    Task<Response<string>> UpdateAsync(UpdateDepartmentRequest request, string departmentId, CancellationToken cts = default);
    Task<Response<string>> DeleteAsync(string id, CancellationToken cts = default);
    Task<Response<EmployeeResponse>> GetManagerAsync(CancellationToken cts = default);
    Task<Response<string>> SetManagerAsync(string id, string managerId, string callerId, string callerRole, CancellationToken cts = default);
    Task<Response<string>> UnsetManagerAsync(string id, string callerId, string callerRole, CancellationToken cts = default);
}


public sealed record CreateDepartmentRequest
{
    [Required]
    public string Name { get; init; } = default!;
}

public sealed record UpdateDepartmentRequest
{
    public string Name { get; init; } = default!;
}


public sealed record DepartmentResponse(
    string Id,
    string Name,
    string CompanyId,
    string? ManagerName,
    DateTime CreatedAt
);


public static class DepartmentMappings
{
    public static DepartmentResponse ToResponse(this Department department)
    {
        return new(
            department.Id,
            department.Name,
            department.CompanyId,
            department.Manager?.GetFullName(),
            department.CreatedAt
        );
    }
}

