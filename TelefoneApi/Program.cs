using TelefoneApi.Middleware;
using TelefoneApi.Services;
using TelefoneApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// ── CPF — Direct Data (token via query param) ─────────────────────────────────
var directDataUrl = builder.Configuration["DirectDataUrl"]
    ?? "https://apiv3.directd.com.br";

builder.Services.AddHttpClient<ICpfTelefoneService, CpfTelefoneService>(client =>
{
    client.BaseAddress = new Uri(directDataUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ── CNPJ — BrasilAPI (sem autenticação, gratuita) ─────────────────────────────
var brasilApiUrl = builder.Configuration["BrasilApiUrl"]
    ?? "https://brasilapi.com.br";

builder.Services.AddHttpClient<ICnpjTelefoneService, CnpjTelefoneService>(client =>
{
    client.BaseAddress = new Uri(brasilApiUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<BearerAuthMiddleware>();

// ── Health ────────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// ── CPF ───────────────────────────────────────────────────────────────────────
app.MapGet("/consulta/cpf/{cpf}", async (
    string cpf,
    ICpfTelefoneService service,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var digits = CpfValidator.Strip(cpf);

    if (!CpfValidator.Validate(digits))
        return Results.Json(new { error = "CPF inválido" }, statusCode: 400);

    try
    {
        var result = await service.ConsultarAsync(digits, cancellationToken);
        return Results.Ok(result);
    }
    catch (CpfTelefoneNotFoundException)
    {
        return Results.Json(new { error = "CPF não encontrado" }, statusCode: 404);
    }
    catch (CpfTelefoneServiceUnavailableException ex)
    {
        logger.LogError("Serviço indisponível (CPF): {Message}", ex.Message);
        return Results.Json(new { error = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro interno ao consultar telefones do CPF");
        return Results.Json(new { error = "Erro interno do servidor" }, statusCode: 500);
    }
});

// ── CNPJ (catch-all trata CNPJ formatado com barra) ──────────────────────────
app.MapGet("/consulta/cnpj/{**cnpj}", async (
    string cnpj,
    ICnpjTelefoneService service,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var digits = CnpjValidator.Strip(cnpj);

    if (!CnpjValidator.Validate(digits))
        return Results.Json(new { error = "CNPJ inválido" }, statusCode: 400);

    try
    {
        var result = await service.ConsultarAsync(digits, cancellationToken);
        return Results.Ok(result);
    }
    catch (CnpjTelefoneNotFoundException)
    {
        return Results.Json(new { error = "CNPJ não encontrado" }, statusCode: 404);
    }
    catch (CnpjTelefoneServiceUnavailableException ex)
    {
        logger.LogError("Serviço indisponível (CNPJ): {Message}", ex.Message);
        return Results.Json(new { error = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro interno ao consultar telefones do CNPJ");
        return Results.Json(new { error = "Erro interno do servidor" }, statusCode: 500);
    }
});

app.Run();

public partial class Program { }
