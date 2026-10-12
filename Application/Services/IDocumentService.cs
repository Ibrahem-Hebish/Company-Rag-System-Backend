using Application.ResultPattern;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Services;

public interface IDocumentService
{
    Task<Response<string>> UploadAsync(UploadDocumentRequest request, string uploaderId, CancellationToken cancellationToken = default);
    Task<Response<DocumentFileResponse>> GetByIdAsync(string documentId, CancellationToken cancellationToken = default);
    Task<Response<List<DocumentResponse>>> GetByDepartmentIdAsync(CancellationToken cancellationToken = default);
    Task<Response<List<DocumentResponse>>> GetByUploaderIdAsync(string uploaderId, CancellationToken cancellationToken = default);
    Task<Response<List<DocumentResponse>>> GetByCompanyIdAsync(string companyId, CancellationToken cancellationToken = default);
    Task<Response<string>> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);
    Task<Response<string>> SemanticSearch(string query, CancellationToken cts = default);
    Task<Response<List<DocumentChunckResponse>>> SearchForRelativeChunks(string query, CancellationToken cts = default);
}


public sealed record UploadDocumentRequest
{
    [Required]
    public string Name { get; init; } = default!;
    [Required]
    [FileValidation]
    public IFormFile File { get; init; } = default!;
}


// File Validation Attribute
public class FileValidationAttribute : ValidationAttribute
{
    private readonly string[] allowedExtensions = { ".pdf", ".docx", ".xlsx", ".txt", ".md" };
    private readonly long maxFileSize = 50 * 1024 * 1024; // 50 MB
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
            return new ValidationResult("No file uploaded.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return new ValidationResult($"Invalid file type. Allowed types are: {string.Join(", ", allowedExtensions)}");

        if (file.Length > maxFileSize)
            return new ValidationResult($"File size exceeds the maximum limit of {maxFileSize / (1024 * 1024)} MB.");

        return ValidationResult.Success;
    }
}




public sealed record DocumentResponse(
    string Id,
    string Name,
    string FilePath,
    string DepartmentName,
    string UploaderName,
    DateTime UploadedAt
);

public sealed record DocumentFileResponse(
    string Id,
    string Name,
    byte[] FileContent,
    string DepartmentName,
    string UploaderName,
    DateTime UploadedAt
);


public static class DocumentMappings
{
    public static DocumentResponse ToResponse(this Document document)
    {
        return new(
            document.Id,
            document.Name,
            document.Path,
            document.Department.Name,
            document.Uploader?.GetFullName()!,
            document.UploadedAt
        );
    }

}


public sealed record DocumentChunckResponse(
    string Id,
    string Content,
    double? Score,
    string DocumentId,
    string DepartmentId);


public sealed record NewDocumentIsSavedEvent(string FilePath, string DepartmentId, string DocumentId);
public sealed record DocumentIsDeletedEvent(string FilePath, string DepartmentId, string DocumentId);
public sealed record DocumentsDepartmentIsDeletedEvent(string DepartmentId, List<string> FilePaths, List<string> DocumentIds);
