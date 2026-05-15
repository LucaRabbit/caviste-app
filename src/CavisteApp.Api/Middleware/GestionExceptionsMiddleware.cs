using System.Net;
using System.Text;
using System.Text.Json;

namespace CavisteApp.Api.Middleware;

public class GestionExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GestionExceptionsMiddleware> _logger;

    public GestionExceptionsMiddleware(RequestDelegate next, ILogger<GestionExceptionsMiddleware> logger)
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
        catch (ArgumentException ex)
        {
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorResponseAsync(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non gérée");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, "Une erreur serveur inattendue est survenue.");
        }
    }
    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode code, string message)
    {
        context.Response.StatusCode = (int)code;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(payload, Encoding.UTF8);
    }
}