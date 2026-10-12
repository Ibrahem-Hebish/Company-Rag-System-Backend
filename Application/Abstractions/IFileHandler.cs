using Microsoft.AspNetCore.Http;

namespace Application.Abstractions;

public interface IFileHandler
{
    Task<string> SaveAsync(IFormFile file, string directoryPath, CancellationToken cts = default);
    void Delete(string filePath);

    Task<byte[]> ReadAsync(string filePath, CancellationToken cts = default);
}
