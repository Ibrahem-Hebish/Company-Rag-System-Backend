using System.Threading.Channels;

namespace Infrustructure.Abstractions;

public sealed class DocumentChannel(
    Channel<NewDocumentIsSavedEvent> newDocumentIsSavedChannel,
    Channel<DocumentIsDeletedEvent> documentIsDeletedChannel,
    Channel<DocumentsDepartmentIsDeletedEvent> documentsDepartmentDeletedChannel,
    Channel<CompanyIsDeletedEvent> companyIsDeletedChannel) : IDocumentChannel
{
    public async Task<NewDocumentIsSavedEvent> ReadDocumentIsSavedAsync(CancellationToken cts = default)
    {
        var @event = await newDocumentIsSavedChannel.Reader.ReadAsync(cts);

        return @event;
    }

    public async Task WriteDocumentIsSavedAsync(NewDocumentIsSavedEvent @event
        , CancellationToken cts = default)
    {
        await newDocumentIsSavedChannel.Writer.WriteAsync(@event, cts);
    }

    public async Task WriteDocumnetsDepartmentIsDeletedAsync(DocumentsDepartmentIsDeletedEvent @event
        , CancellationToken xts = default)
    {
        await documentsDepartmentDeletedChannel.Writer.WriteAsync(@event, xts);
    }

    public async Task WriteDocumentIsDeletedAsync(DocumentIsDeletedEvent @event
        , CancellationToken cts = default)
    {
        await documentIsDeletedChannel.Writer.WriteAsync(@event, cts);
    }

    public async Task<DocumentIsDeletedEvent> ReadDocumentIsDeletedAsync(CancellationToken cts = default)
    {
        var @event = await documentIsDeletedChannel.Reader.ReadAsync(cts);

        return @event;
    }

    public async Task<DocumentsDepartmentIsDeletedEvent> ReadDocumentsDepartmentIsDeletedAsync(
        CancellationToken cts = default)
    {
        var @event = await documentsDepartmentDeletedChannel.Reader.ReadAsync(cts);

        return @event;
    }

    public async Task WriteCompanyIsDeletedAsync(CompanyIsDeletedEvent @event
        ,CancellationToken cts = default)
    {
        await companyIsDeletedChannel.Writer.WriteAsync(@event, cts);
    }

    public async Task<CompanyIsDeletedEvent> ReadCompanyIsDeletedAsync(CancellationToken cts = default)
    {
        var @event = await companyIsDeletedChannel.Reader.ReadAsync(cts);

        return @event;
    }
}