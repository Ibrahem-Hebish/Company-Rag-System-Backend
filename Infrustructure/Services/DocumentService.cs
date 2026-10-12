using Document = Domain.Entities.Document;

namespace Infrustructure.Services;

public sealed class DocumentService(
    AppDbContext dbContext,
    ITenantResolver tenantResolver,
    IFileHandler fileHandler,
    IDocumentChannel documentChannel,
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    ILogger<DocumentService> logger)

    : ResponseHandler, IDocumentService
{
    public async Task<Response<string>> DeleteDocumentAsync(string documentId, CancellationToken cts = default)
    {
        var document = await dbContext.Documents.FindAsync(documentId, cts);

        if (document is null)
            return NotFound<string>("There is no document");

        try
        {
            dbContext.Documents.Remove(document);

            await dbContext.SaveChangesAsync(cts);

            await documentChannel.WriteDocumentIsDeletedAsync(
                new DocumentIsDeletedEvent(document.Path, document.DepartmentId, document.Id),
                cts);
        }
        catch(Exception ex)
        {
            // Log the exception

            throw new Exception(ex.Message);
        }

        return Deleted<string>("Document is deleted successfully.");
    }

    public async Task<Response<List<DocumentResponse>>> GetByCompanyIdAsync(string companyId, CancellationToken cts = default)
    {
        var currentCompanyId = tenantResolver.GetCompanyId();

        var documents = await dbContext.Documents
            .AsNoTracking()
            .Where(d => d.CompanyId == currentCompanyId)
            .Select(d => new DocumentResponse(
                d.Id,
                d.Name,
                d.Path,
                d.Department.Name,
                d.Uploader != null ? d.Uploader.GetFullName() : "unknown",
                d.UploadedAt))
            .ToListAsync(cts);

        if (documents is null || documents.Count == 0)
            return NotFound<List<DocumentResponse>>("No documents found for the specified company.");

        return Success(documents);
    }

    public async Task<Response<List<DocumentResponse>>> GetByDepartmentIdAsync( CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();
        var departmentId = tenantResolver.GetDepartmentId();

        var documents = await dbContext.Documents
            .AsNoTracking()
            .Select(d => new DocumentResponse(
                d.Id,
                d.Name,
                d.Path,
                d.Department.Name,
                d.Uploader != null ? d.Uploader.GetFullName() : "unknown",
                d.UploadedAt))
            .ToListAsync(cts);

        if(documents is null || documents.Count == 0)
            return NotFound<List<DocumentResponse>>("No documents found for the specified department.");

        return Success(documents);
    }

    public async Task<Response<DocumentFileResponse>> GetByIdAsync(string documentId, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var document = await dbContext.Documents
                                      .Where(d => d.Id == documentId)
                                      .Select(d => new
                                      {
                                          d.Id,
                                          d.Name,
                                          d.Path,
                                          DepartmentName = d.Department.Name,
                                          UploaderFirstName = d.Uploader != null ? d.Uploader.FirstName : "",
                                          UploaderLastName = d.Uploader != null ? d.Uploader.LastName : "Unknown",
                                          d.UploadedAt
                                      })
                                      .FirstOrDefaultAsync(cts);

        if (document is null)
            return NotFound<DocumentFileResponse>("Document not found.");

        var fileContent = await fileHandler.ReadAsync(document.Path, cts);

        var documentResponse = new DocumentFileResponse(
            Id: document.Id,
            Name: document.Name,
            FileContent: fileContent,
            DepartmentName: document.DepartmentName,
            UploaderName: document.UploaderFirstName + " " + document.UploaderLastName,
            UploadedAt: document.UploadedAt
        );

        return Success(documentResponse);
    }

    public async Task<Response<List<DocumentResponse>>> GetByUploaderIdAsync(string uploaderId, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var documents = await dbContext.Documents
            .AsNoTracking()
            .Where(d => d.UploaderId == uploaderId)
            .Select(d => new DocumentResponse(
                d.Id,
                d.Name,
                d.Path,
                d.Department.Name,
                d.Uploader != null ? d.Uploader.GetFullName() : "unknown",
                d.UploadedAt))
            .ToListAsync(cts);

        if (documents is null || documents.Count == 0)
            return NotFound<List<DocumentResponse>>("No documents found for the specified uploader.");

        return Success(documents);
    }

    public async Task<Response<string>> UploadAsync(UploadDocumentRequest request, string uploaderId, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();
        var departmentId = tenantResolver.GetDepartmentId();

        var department = await dbContext.Departments
                                        .Include(d => d.Company)
                                        .FirstOrDefaultAsync(d => d.Id == departmentId, cts);

        if (department is null)
            return NotFound<string>("Department does not exsist;");

        var directoryPath = $"{department.Company.Id}-{department.Name}";

        var documentPath = await fileHandler.SaveAsync(request.File, directoryPath, cts);

        var document = Document.Create(
            request.Name,
            documentPath,
            departmentId,
            uploaderId,
            companyId);

        await dbContext.Documents.AddAsync(document, cts);

        try
        {
            await dbContext.SaveChangesAsync(cts);

        } catch (Exception ex)
        {
            logger.LogError("error while uploading document: {ex}", ex.Message);

            fileHandler.Delete(documentPath);

            throw new Exception(ex.Message);
        }

        await documentChannel.WriteDocumentIsSavedAsync(
            new NewDocumentIsSavedEvent(document.Path, document.DepartmentId, document.Id)
            , cts);

        return Created<string>("Document is created successfully.");
    }

    public async Task<Response<string>> SemanticSearch(string query, CancellationToken cts = default)
    {
        var departmentId = tenantResolver.GetDepartmentId();

        var retrievedChuncks = await GetRelativeChuncksAsync(query, departmentId,cts);

        StringBuilder context = new();

        await foreach (var chunck in retrievedChuncks.WithCancellation(cts))
        {
            context.AppendLine($"[Document Chunk with score {chunck.Score}]");
            context.AppendLine(chunck.Record.Content);
            context.AppendLine();
        }

        if (string.IsNullOrWhiteSpace(context.ToString()))
            return Success("I don't have enough information in the provided documents.");

        IEnumerable<ChatMessage> chatMessages =
        [
            new ChatMessage
            {
                Role = ChatRole.System,
                Contents =
                [
                    new TextContent($"""
                        You are a helpful assistant answering questions using the provided context.

                        Rules:
                        - Answer the user's question using the provided context.
                        - If the answer cannot be found in the context, say that you don't have enough information.
                        - Do not invent or assume facts that are not supported by the context.
                        - Keep the answer relevant and concise.

                        Retrieved context: {context}
                        """)
                ]
            },

            new ChatMessage
            {
                Role = ChatRole.User,
                Contents =
                [
                    new TextContent($"""
                        User question:
                        {query}
                        """)
                ]
            }
        ];

        var chatResponse = await chatClient.GetResponseAsync(
            chatMessages,
            cancellationToken: cts);

        return Success(chatResponse.Text);
    }

    public async Task<Response<List<DocumentChunckResponse>>> SearchForRelativeChunks(string query, CancellationToken cts = default)
    {
        var departmentId = tenantResolver.GetDepartmentId();

        var retrievedChuncks = await GetRelativeChuncksAsync(query, departmentId, cts);

        if (!await retrievedChuncks.AnyAsync(cts))
            return NotFound<List<DocumentChunckResponse>>("There is no relevant information.");

        var response = new List<DocumentChunckResponse>();

        await foreach(var chunck in retrievedChuncks.WithCancellation(cts))
        {
            response.Add(new(
                chunck.Record.Id,
                chunck.Record.Content,
                chunck.Score,
                chunck.Record.DocumentId,
                chunck.Record.DepartmentId));
        }

        return Success(response);
    }

    private async Task<IAsyncEnumerable<VectorSearchResult<DocumentChunk>>> GetRelativeChuncksAsync(string query, string departmentId, CancellationToken cts = default)
    {
        var embeddingsOptions = new EmbeddingGenerationOptions()
        {
            Dimensions = 1024
        };

        var queryEmbeddings = await embeddingGenerator.GenerateVectorAsync(query, embeddingsOptions, cts);

        var collection = vectorStore.GetCollection<string, DocumentChunk>("document_chunks");

        await collection.EnsureCollectionExistsAsync(cts);

        var vectorSearchOptions = new VectorSearchOptions<DocumentChunk>()
        {
            Filter = d => d.DepartmentId == departmentId,
        };

        var retrievedChuncks = collection.SearchAsync(queryEmbeddings, 5, vectorSearchOptions, cts);

        return retrievedChuncks;
    }

}
