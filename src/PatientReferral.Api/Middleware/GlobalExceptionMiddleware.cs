using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PatientReferral.Application.Exceptions;

namespace PatientReferral.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogWarning(ex, "Resource not found. TraceId: {TraceId}", traceId);
            await WriteProblemDetailsAsync(context, HttpStatusCode.NotFound, ex.Message, traceId);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
            await WriteProblemDetailsAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.", traceId);
        }
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, HttpStatusCode statusCode, string title, string traceId)
    {
        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Type = GetProblemTypeUri(statusCode)
        };

        problem.Extensions["traceId"] = traceId;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static string GetProblemTypeUri(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",
            HttpStatusCode.NotFound => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5",
            HttpStatusCode.InternalServerError => "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1",
            _ => "https://www.rfc-editor.org/rfc/rfc9110"
        };
    }
}
