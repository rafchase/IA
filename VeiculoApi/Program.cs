using VeiculoApi.Middleware;
using VeiculoApi.Services;
using VeiculoApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// ── apiplacas.com.br — token via query param ──────────────────────────────────
var apiPlacasUrl = builder.Configuration["ApiPlacasUrl"]
    ?? "https://www.apiplacas.com.br";

builder.Services.AddHttpClient<IVeiculoService, VeiculoService>(client =>
{
    client.BaseAddress = new Uri(apiPlacasUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<BearerAuthMiddleware>();

// ── Health ────────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// ── Placa ─────────────────────────────────────────────────────────────────────
app.MapGet("/consulta/placa/{placa}", async (
    string placa,
    IVeiculoService veiculoService,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    if (!PlacaValidator.Validate(placa))
        return Results.Json(
            new { error = "Placa inválida. Use o formato ABC1234 (antigo) ou ABC1D23 (Mercosul)." },
            statusCode: 400);

    try
    {
        var result = await veiculoService.ConsultarAsync(placa, cancellationToken);
        return Results.Ok(result);
    }
    catch (VeiculoNotFoundException)
    {
        return Results.Json(new { error = "Placa não encontrada" }, statusCode: 404);
    }
    catch (VeiculoServiceUnavailableException ex)
    {
        logger.LogError("Serviço indisponível: {Message}", ex.Message);
        return Results.Json(new { error = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro interno ao consultar placa");
        return Results.Json(new { error = "Erro interno do servidor" }, statusCode: 500);
    }
});

app.Run();

public partial class Program { }
