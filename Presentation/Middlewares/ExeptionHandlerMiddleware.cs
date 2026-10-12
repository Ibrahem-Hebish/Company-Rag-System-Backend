using System.Text.Json;

namespace Presentation.Middlewares;

public class ExeptionHandlerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails()
            {
                Status = context.Response.StatusCode == 200 ? StatusCodes.Status400BadRequest : context.Response.StatusCode,
                Detail = ex.InnerException?.Message ?? ex.Message,
                Title = "An error occurred while processing your request.",
                Instance = context.Request.Path,
            };

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
