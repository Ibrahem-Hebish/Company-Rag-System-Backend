using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace Infrustructure;

public class DocumentChunk
{
    [VectorStoreKey]
    public string Id { get; set; }
    [VectorStoreData]
    public string Content { get; set; }
    [VectorStoreVector(1024, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Embeddings { get; set; }
    [VectorStoreData(IsIndexed = true)]
    public string DocumentId { get; set; }
    [VectorStoreData]
    public string DepartmentId { get; set; }

    public DocumentChunk() { }

    public DocumentChunk(string content, ReadOnlyMemory<float> embeddingsVector, string documentId, string departmentId)
    {
        Id = Guid.NewGuid().ToString();
        Content = content;
        Embeddings = embeddingsVector;
        DocumentId = documentId;
        DepartmentId = departmentId;
    }

}





public interface IDocumentChunkRepository
{
    Task DeleteByDocumentIdAsync(
        string documentId,
        CancellationToken cancellationToken = default);
    Task DeleteByDepartmentIdAsync(
        string departmentId,
        CancellationToken cancellationToken = default);
}