namespace Infrustructure.EventConsumers;

public sealed class DocumentIsSavedEventConsumer(
    IDocumentChannel documentChannel,
    IWebHostEnvironment webHostEnvironment,
    ILogger<DocumentIsSavedEventConsumer> logger,
    IngestionDocumentReader reader,
    IngestionChunker<string> chunker,
    ClassificationEnricher classifier,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IOptions<AIOptions> aiOptions,
    VectorStore vectorStore) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var collection = vectorStore.GetCollection<string, DocumentChunk>("document_chunks");
        await collection.EnsureCollectionExistsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var @event = await documentChannel.ReadDocumentIsSavedAsync(stoppingToken);

            if (@event is null) continue;

            try
            {
                await IngestAsync(@event, collection, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to ingest document {DocumentId}", @event.DocumentId);
            }
        }
    }

    private async Task IngestAsync(
        NewDocumentIsSavedEvent @event,
        VectorStoreCollection<string, DocumentChunk> collection,
        CancellationToken ct)
    {
        var fullPath = Path.Combine(
                   webHostEnvironment.WebRootPath,
                   @event.FilePath);

        // MarkItDownReader parses the actual file
        var ingestionDocument = await reader.ReadAsync(
            new FileInfo(fullPath),
            ct);

        var chuncks = chunker.ProcessAsync(ingestionDocument, ct);

        var classifiedChunks = classifier.ProcessAsync(chuncks, ct);

        await foreach (var chunk in classifiedChunks.WithCancellation(ct))
        {
            var embeddingGeneratorOptons = new EmbeddingGenerationOptions()
            {
                Dimensions = aiOptions.Value.EmbeddingDimentions
            };

            var chunkEmbeddings = await embeddingGenerator.GenerateVectorAsync(chunk.Content,
                embeddingGeneratorOptons, ct);

            var documentChunk = new DocumentChunk(chunk.Content.ToString(), chunkEmbeddings
                , @event.DocumentId, @event.DepartmentId);

            await collection.UpsertAsync(documentChunk, ct);
        }
    }
}
