using SerasaApi.Middleware;
using SerasaApi.Services;
using SerasaApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// ── Named HttpClient for the OAuth2 token endpoint (used by singleton) ────────
var serasaTokenUrl = builder.Configuration["SerasaTokenUrl"]
    ?? "https://sandbox.serasaexperian.com.br/security/iam/v2/client-token";

builder.Services.AddHttpClient("SerasaToken", client =>
{
    client.BaseAddress = new Uri(serasaTokenUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Singleton: token is cached and shared across requests
builder.Services.AddSingleton<ISerasaTokenService, SerasaTokenService>();

// ── CPF service ───────────────────────────────────────────────────────────────
var serasaCpfUrl = builder.Configuration["SerasaCpfUrl"]
    ?? "https://sandbox.serasaexperian.com.br/insights-bureau-reports/v1/pf/reports";

builder.Services.AddHttpClient<ICpfSerasaService, CpfSerasaService>(client =>
{
    client.BaseAddress = new Uri(serasaCpfUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ── CNPJ service ──────────────────────────────────────────────────────────────
var serasaCnpjUrl = builder.Configuration["SerasaCnpjUrl"]
    ?? "https://sandbox.serasaexperian.com.br/insights-bureau-reports/v1/pj/reports";

builder.Services.AddHttpClient<ICnpjSerasaService, CnpjSerasaService>(client =>
{
    client.BaseAddress = new Uri(serasaCnpjUrl);
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
    ICpfSerasaService cpfService,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var digits = CpfValidator.Strip(cpf);

    if (!CpfValidator.Validate(digits))
        return Results.Json(new { error = "CPF inválido" }, statusCode: 400);

    try
    {
        var result = await cpfService.ConsultarAsync(digits, cancellationToken);
        return Results.Ok(result);
    }
    catch (CpfSerasaNotFoundException)
    {
        return Results.Json(new { error = "CPF não encontrado na Serasa" }, statusCode: 404);
    }
    catch (CpfSerasaServiceUnavailableException ex)
    {
        logger.LogError("Serviço Serasa indisponível (CPF): {Message}", ex.Message);
        return Results.Json(new { error = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro interno ao consultar CPF na Serasa");
        return Results.Json(new { error = "Erro interno do servidor" }, statusCode: 500);
    }
});

// ── CNPJ (catch-all handles formatted CNPJ with slash) ────────────────────────
app.MapGet("/consulta/cnpj/{**cnpj}", async (
    string cnpj,
    ICnpjSerasaService cnpjService,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var digits = CnpjValidator.Strip(cnpj);

    if (!CnpjValidator.Validate(digits))
        return Results.Json(new { error = "CNPJ inválido" }, statusCode: 400);

    try
    {
        var result = await cnpjService.ConsultarAsync(digits, cancellationToken);
        return Results.Ok(result);
    }
    catch (CnpjSerasaNotFoundException)
    {
        return Results.Json(new { error = "CNPJ não encontrado na Serasa" }, statusCode: 404);
    }
    catch (CnpjSerasaServiceUnavailableException ex)
    {
        logger.LogError("Serviço Serasa indisponível (CNPJ): {Message}", ex.Message);
        return Results.Json(new { error = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro interno ao consultar CNPJ na Serasa");
        return Results.Json(new { error = "Erro interno do servidor" }, statusCode: 500);
    }
});

app.Run();

public partial class Program { }
