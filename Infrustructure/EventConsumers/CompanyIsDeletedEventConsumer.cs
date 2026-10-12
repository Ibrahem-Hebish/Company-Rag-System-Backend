
namespace Infrustructure.EventConsumers;

public sealed class CompanyIsDeletedEventConsumer(
    IFileHandler fileHandler,
    IDocumentChannel documentChannel,
    IServiceScopeFactory serviceScopeFactory,
    IWebHostEnvironment webHostEnvironment,
    ILogger<CompanyIsDeletedEventConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       
        while (!stoppingToken.IsCancellationRequested)
        {
            var @event = await documentChannel.ReadCompanyIsDeletedAsync(stoppingToken);

            try
            {

                logger.LogInformation(
                    "Processing company deletion: {CompanyId}",
                    @event.CompanyId);


                foreach(var filePath in @event.FilePaths)
                {
                    var fullPath = Path.Combine(
                           webHostEnvironment.WebRootPath,
                           filePath);

                    fileHandler.Delete(fullPath);
                }


                var scope = serviceScopeFactory.CreateAsyncScope();

                var documentChunkRepository = scope.ServiceProvider.GetRequiredService<IDocumentChunkRepository>();

                foreach(var departmentId in @event.DepartmentIds)
                    await documentChunkRepository.DeleteByDepartmentIdAsync(departmentId, stoppingToken);

                var dbcontext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var companyUsers = await dbcontext.Users.Where(u => @event.EmployeesIds.Contains(u.Id))
                                                        .ToListAsync(stoppingToken);

                dbcontext.Users.RemoveRange(companyUsers);

                await dbcontext.SaveChangesAsync(stoppingToken);

                logger.LogInformation(
                    "Company deletion completed: {CompanyId}",
                    @event.CompanyId);
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
                    "Failed to process company deletion: {CompanyId}",
                    @event.CompanyId);
            }
        }
    }
}
