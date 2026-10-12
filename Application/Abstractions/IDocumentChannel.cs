using Application.Services;

namespace Application.Abstractions;

public interface IDocumentChannel
{
    Task WriteDocumentIsSavedAsync(NewDocumentIsSavedEvent @event, CancellationToken cts = default);
    Task<NewDocumentIsSavedEvent> ReadDocumentIsSavedAsync(CancellationToken cts = default);
    Task WriteDocumentIsDeletedAsync(DocumentIsDeletedEvent @event, CancellationToken cts = default);
    Task<DocumentIsDeletedEvent> ReadDocumentIsDeletedAsync(CancellationToken cts = default);
    Task WriteDocumnetsDepartmentIsDeletedAsync(DocumentsDepartmentIsDeletedEvent @event, CancellationToken cts = default);
    Task<DocumentsDepartmentIsDeletedEvent> ReadDocumentsDepartmentIsDeletedAsync(CancellationToken cts = default);
    Task WriteCompanyIsDeletedAsync(CompanyIsDeletedEvent @event, CancellationToken cts = default);
    Task<CompanyIsDeletedEvent> ReadCompanyIsDeletedAsync(CancellationToken cts = default);
}