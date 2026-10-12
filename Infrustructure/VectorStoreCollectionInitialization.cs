
namespace Infrustructure;
public static class VectorStoreCollectionInitialization
{
    public static async Task InitializeVectorStoreAsync(
        this IServiceProvider services)
    {
        try
        {
            var vectorStore = services.GetRequiredService<VectorStore>();


            var collection = vectorStore.GetCollection<string, DocumentChunk>(
                "document_chunks");

            await collection.EnsureCollectionExistsAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error initializing vector store collection: {ex.Message}");
        }
    }
}



