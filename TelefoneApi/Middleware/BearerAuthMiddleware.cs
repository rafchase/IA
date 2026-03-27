using System.Net;
using System.Text.Json;

namespace TelefoneApi.Middleware;

public class BearerAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BearerAuthMiddleware> _logger;

    public BearerAuthMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<BearerAuthMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth for health check
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var expectedToken = _configuration["ApiToken"];
        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            _logger.LogError("ApiToken não configurado");
            await WriteUnauthorized(context, "Serviço não configurado corretamente");
            return;
        }

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        var token = ExtractBearerToken(authHeader);

        if (string.IsNullOrEmpty(token) || token != expectedToken)
        {
            _logger.LogWarning("Requisição com token inválido ou ausente");
            await WriteUnauthorized(context, "Token inválido ou ausente");
            return;
        }

        await _next(context);
    }

    private static string? ExtractBearerToken(string? header)
    {
        if (string.IsNullOrWhiteSpace(header)) return null;
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;
        var token = header["Bearer ".Length..].Trim();
        return string.IsNullOrEmpty(token) ? null : token;
    }

    private static async Task WriteUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(body);
    }
}
