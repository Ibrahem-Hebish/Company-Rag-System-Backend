
namespace Infrustructure.EventConsumers;

public sealed class DocumentIsDeletedEventConsumer(
    IFileHandler fileHandler,
    IDocumentChannel documentChannel,
    IServiceScopeFactory serviceScopeFactory,
    IWebHostEnvironment webHostEnvironment,
    ILogger<DocumentIsDeletedEventConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       


        while (!stoppingToken.IsCancellationRequested)
        {
            var @event = await documentChannel.ReadDocumentIsDeletedAsync(stoppingToken);

            try
            {

                logger.LogInformation(
                    "Processing document deletion: {DocumentId}",
                    @event.DocumentId);


                var fullPath = Path.Combine(
                           webHostEnvironment.WebRootPath,
                           @event.FilePath);

                fileHandler.Delete(fullPath);

                var scope = serviceScopeFactory.CreateAsyncScope();

                var documentChunkRepository = scope.ServiceProvider.GetRequiredService<IDocumentChunkRepository>();

                await documentChunkRepository.DeleteByDocumentIdAsync(@event.DocumentId, stoppingToken);

                logger.LogInformation(
                    "Document deletion completed: {DocumentId}",
                    @event.DocumentId);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to process document deletion: {DocumentId}",
                    @event.DocumentId);
            }
        }
    }
}
