using System.Net;
using System.Text.Json;
using ECommerceInventory.Application.DTOs.Common;

namespace ECommerceInventory.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        context.Response.Headers.Append("X-Correlation-ID", correlationId);
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex, correlationId);
            }
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex, string correlationId)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ApiResponse<object>.FailureResponse(
            $"An unexpected error occurred. Please try again later. CorrelationId: {correlationId}");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        await ctx.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
