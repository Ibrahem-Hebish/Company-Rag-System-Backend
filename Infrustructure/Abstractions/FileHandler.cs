using Microsoft.AspNetCore.Hosting;

namespace Infrustructure.Abstractions;

public sealed class FileHandler(IWebHostEnvironment webHostEnvironment) : IFileHandler
{
    public void Delete(string filePath)
    {
        var fullPath = Path.Combine(webHostEnvironment.WebRootPath, filePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public async Task<byte[]> ReadAsync(string filePath, CancellationToken cts = default)
    {
        var fullPath = Path.Combine(webHostEnvironment.WebRootPath, filePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.");

        return await File.ReadAllBytesAsync(fullPath, cts);
    }

    public async Task<string> SaveAsync(IFormFile file, string directoryPath, CancellationToken cts = default)
    {
        var uploadsFolderPath = Path.Combine(webHostEnvironment.WebRootPath, directoryPath);

        if (!Directory.Exists(uploadsFolderPath))
            Directory.CreateDirectory(uploadsFolderPath);

        var fileExtension = Path.GetExtension(file.FileName);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileExtension}";

        var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(fileStream, cts);

        return $"{directoryPath}/{uniqueFileName}";
    }
}
