namespace Presentation.Controllers;

[Route("api/v{version:apiversion}/[controller]")]
[ApiController]
public class AppControllerBase : ControllerBase
{

    protected IActionResult NewResponse<T>(T response) where T : IResponse
    {
        return response.StatusCode switch
        {
            HttpStatusCode.OK => Ok(response),
            HttpStatusCode.NoContent => Ok(response),
            HttpStatusCode.InternalServerError => new ObjectResult(response),
            HttpStatusCode.Created => Created(nameof(Response), response),
            HttpStatusCode.BadRequest => BadRequest(response),
            HttpStatusCode.Unauthorized => Unauthorized(response),
            HttpStatusCode.NotFound => NotFound(response),
            _ => StatusCode((int)response.StatusCode, response),
        };
    }
}
