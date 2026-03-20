using CpfApi.Middleware;
using CpfApi.Services;
using CpfApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// HttpClient for ReceitaWS CPF
var receitaWsUrl = builder.Configuration["ReceitaWsUrl"]
    ?? "https://www.receitaws.com.br/v1/cpf";

builder.Services.AddHttpClient<ICpfService, CpfService>(client =>
{
    client.BaseAddress = new Uri(receitaWsUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// HttpClient for ReceitaWS CNPJ
var receitaWsCnpjUrl = builder.Configuration["ReceitaWsCnpjUrl"]
    ?? "https://www.receitaws.com.br/v1/cnpj";

builder.Services.AddHttpClient<ICnpjService, CnpjService>(client =>
{
    client.BaseAddress = new Uri(receitaWsCnpjUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<BearerAuthMiddleware>();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// CPF lookup
app.MapGet("/consulta/cpf/{cpf}", async (
    string cpf,
    ICpfService cpfService,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var digits = CpfValidator.Strip(cpf);

    if (!CpfValidator.Validate(digits))
    {
        return Results.Json(
            new { error = "CPF inválido" },
            statusCode: 400);
    }

    try
    {
        var result = await cpfService.ConsultarAsync(digits, cancellationToken);
        return Results.Ok(result);
    }
    catch (CpfNotFoundException)
    {
        return Results.Json(new { error = "CPF não encontrado" }, statusCode: 404);
    }
    catch (CpfServiceUnavailableException ex)
    {
        logger.LogError("Serviço indisponível: {Message}", ex.Message);
        return Results.Json(new { error = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro interno ao consultar CPF");
        return Results.Json(new { error = "Erro interno do servidor" }, statusCode: 500);
    }
});

// CNPJ lookup
app.MapGet("/consulta/cnpj/{**cnpj}", async (
    string cnpj,
    ICnpjService cnpjService,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var digits = CnpjValidator.Strip(cnpj);

    if (!CnpjValidator.Validate(digits))
    {
        return Results.Json(
            new { error = "CNPJ inválido" },
            statusCode: 400);
    }

    try
    {
        var result = await cnpjService.ConsultarAsync(digits, cancellationToken);
        return Results.Ok(result);
    }
    catch (CnpjNotFoundException)
    {
        return Results.Json(new { error = "CNPJ não encontrado" }, statusCode: 404);
    }
    catch (CnpjServiceUnavailableException ex)
    {
        logger.LogError("Serviço indisponível: {Message}", ex.Message);
        return Results.Json(new { error = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro interno ao consultar CNPJ");
        return Results.Json(new { error = "Erro interno do servidor" }, statusCode: 500);
    }
});

app.Run();

public partial class Program { }
