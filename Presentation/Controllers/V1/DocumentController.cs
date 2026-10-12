namespace Presentation.Controllers.V1;

[ApiVersion("1.0")]
[TypeFilter(typeof(DepartmentLevelResolverFilterAttribute))]
[Authorize(Roles = "Owner,CompanyManager,DepartmentManager,Employee")]

public sealed class DocumentController(IDocumentService documentService) : AppControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cts = default)
    {
        var response = await documentService.GetByIdAsync(id, cts);

        return NewResponse(response);
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cts = default)
    {
        var response = await documentService.GetByDepartmentIdAsync(cts);

        return NewResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cts = default)
    {
        var response = await documentService.DeleteDocumentAsync(id, cts);

        return NewResponse(response);
    }

    [HttpGet("semantic-search")]
    public async Task<IActionResult> SemanticSearch(string query, CancellationToken cts = default)
    {
        var response = await documentService.SemanticSearch(query, cts);

        return NewResponse(response);
    }

    [HttpGet("relative-chuncks")]
    public async Task<IActionResult> GetRelativeChuncks(string query, CancellationToken cts = default)
    {
        var response = await documentService.SearchForRelativeChunks(query, cts);

        return NewResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var response = await documentService.UploadAsync(request, id!, cancellationToken);

        return NewResponse(response);
    }
}