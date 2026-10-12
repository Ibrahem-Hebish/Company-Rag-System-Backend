using System.Net;

namespace Application.ResultPattern;

public class PaginatedResponse<T> : IResponse
{
    public T? Data { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
    public string? Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; }

    public PaginatedResponse(T data, int pageNumber, int pageSize, int totalRecords)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        IsSuccess = true;
        StatusCode = HttpStatusCode.OK;
    }

    public PaginatedResponse(string message, HttpStatusCode statusCode)
    {
        Message = message;
        StatusCode = statusCode;
        IsSuccess = false;
    }
}

