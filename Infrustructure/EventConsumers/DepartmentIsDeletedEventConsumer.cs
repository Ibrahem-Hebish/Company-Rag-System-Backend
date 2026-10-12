using Infrustructure;

public sealed class DepartmentIsDeletedEventConsumer(
    IFileHandler fileHandler,
    IDocumentChannel documentChannel,
    IServiceScopeFactory serviceScopeFactory,
    IWebHostEnvironment webHostEnvironment,
    ILogger<DepartmentIsDeletedEventConsumer> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            
            var @event = await documentChannel
                .ReadDocumentsDepartmentIsDeletedAsync(stoppingToken);
           
            try
            {
                logger.LogInformation(
                    "Processing department deletion: {DepartmentId}",
                    @event.DepartmentId);

                foreach (var filePath in @event.FilePaths)
                {
                    var fullPath = Path.Combine(
                        webHostEnvironment.WebRootPath,
                        filePath);

                    fileHandler.Delete(fullPath);
                }

                await using var scope =
                    serviceScopeFactory.CreateAsyncScope();

                var repository = scope.ServiceProvider
                    .GetRequiredService<IDocumentChunkRepository>();

                await repository.DeleteByDepartmentIdAsync(
                    @event.DepartmentId,
                    stoppingToken);

                logger.LogInformation(
                    "Department deletion completed: {DepartmentId}",
                    @event.DepartmentId);
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
                    "Failed to process department deletion: {DepartmentId}",
                    @event.DepartmentId);
            }
        }
    }
}