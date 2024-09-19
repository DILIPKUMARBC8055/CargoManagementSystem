
using CargoManagementProject.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using System.Threading.Tasks;


public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Logger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, Logger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the incoming request
        DateTime requestIn = DateTime.Now;
        _logger.Log($"Incoming Request: {context.Request.Method} {context.Request.Path}");

        await _next(context);

        // Log the outgoing response
        _logger.Log($"Outgoing Response: {context.Response.StatusCode}");
        DateTime responseOut = DateTime.Now;
        TimeSpan difference = responseOut - requestIn;
        _logger.Log($"The total time taken to response :{difference.TotalMilliseconds} milli Seconds\n");

    }
}
