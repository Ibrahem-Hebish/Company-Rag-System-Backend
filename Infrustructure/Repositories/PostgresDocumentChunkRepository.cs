using System.Reflection.Metadata;

namespace Infrustructure.Repositories;

public sealed class PostgresDocumentChunkRepository(
    AppDbContext dbContext) : IDocumentChunkRepository
{
    public async Task DeleteByDepartmentIdAsync(string departmentId, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.ExecuteSqlAsync(
            $"DELETE FROM document_chunks WHERE \"DepartmentId\" = {departmentId}",
            cancellationToken);
    }

    public async Task DeleteByDocumentIdAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.ExecuteSqlAsync(
            $"DELETE FROM document_chunks WHERE \"DocumentId\" = {documentId}",
            cancellationToken);
    }
}