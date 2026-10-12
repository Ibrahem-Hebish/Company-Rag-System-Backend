using System.Net;

namespace Application.ResultPattern;

public interface IResponse
{
    string? Message { get; set; }
    HttpStatusCode StatusCode { get; set; }
    bool IsSuccess { get; set; }
}

