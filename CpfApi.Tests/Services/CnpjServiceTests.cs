using System.Net;
using System.Text;
using System.Text.Json;
using CpfApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CpfApi.Tests.Services;

public class CnpjServiceTests
{
    private static CnpjService BuildService(
        HttpMessageHandler handler,
        string? token = null,
        string baseAddress = "http://fake-receita-ws/v1/cnpj/")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(token is not null
                ? new Dictionary<string, string?> { ["ReceitaWsCnpjToken"] = token }
                : new Dictionary<string, string?>())
            .Build();

        return new CnpjService(httpClient, NullLogger<CnpjService>.Instance, config);
    }

    [Fact]
    public async Task ConsultarAsync_SuccessResponse_ReturnsCnpjResponse()
    {
        var json = JsonSerializer.Serialize(new
        {
            status = "OK",
            cnpj = "11222333000181",
            nome = "EMPRESA TESTE LTDA",
            fantasia = "TESTE",
            situacao = "ATIVA",
            abertura = "01/01/2000"
        });

        var handler = new FakeCnpjHttpHandler(HttpStatusCode.OK, json);
        var service = BuildService(handler);

        var result = await service.ConsultarAsync("11222333000181");

        Assert.Equal("11.222.333/0001-81", result.Cnpj);
        Assert.Equal("EMPRESA TESTE LTDA", result.Nome);
        Assert.Equal("TESTE", result.Fantasia);
        Assert.Equal("ATIVA", result.Situacao);
        Assert.Equal("01/01/2000", result.Abertura);
    }

    [Fact]
    public async Task ConsultarAsync_NotFound_ThrowsCnpjNotFoundException()
    {
        var handler = new FakeCnpjHttpHandler(HttpStatusCode.NotFound, "");
        var service = BuildService(handler);

        await Assert.ThrowsAsync<CnpjNotFoundException>(
            () => service.ConsultarAsync("11222333000181"));
    }

    [Fact]
    public async Task ConsultarAsync_ServerError_ThrowsCnpjServiceUnavailableException()
    {
        var handler = new FakeCnpjHttpHandler(HttpStatusCode.InternalServerError, "");
        var service = BuildService(handler);

        await Assert.ThrowsAsync<CnpjServiceUnavailableException>(
            () => service.ConsultarAsync("11222333000181"));
    }

    [Fact]
    public async Task ConsultarAsync_ErrorStatusInBody_ThrowsCnpjServiceUnavailableException()
    {
        var json = JsonSerializer.Serialize(new
        {
            status = "ERROR",
            message = "CNPJ inválido"
        });

        var handler = new FakeCnpjHttpHandler(HttpStatusCode.OK, json);
        var service = BuildService(handler);

        var ex = await Assert.ThrowsAsync<CnpjServiceUnavailableException>(
            () => service.ConsultarAsync("11222333000181"));

        Assert.Equal("CNPJ inválido", ex.Message);
    }

    [Fact]
    public async Task ConsultarAsync_NetworkError_ThrowsCnpjServiceUnavailableException()
    {
        var handler = new CnpjErrorHttpHandler(new HttpRequestException("connection refused"));
        var service = BuildService(handler);

        await Assert.ThrowsAsync<CnpjServiceUnavailableException>(
            () => service.ConsultarAsync("11222333000181"));
    }

    [Fact]
    public async Task ConsultarAsync_WithToken_AppendsTokenToUrl()
    {
        var capturedUrl = "";
        var json = JsonSerializer.Serialize(new
        {
            status = "OK",
            cnpj = "11222333000181",
            nome = "EMPRESA",
            fantasia = "",
            situacao = "ATIVA",
            abertura = "01/01/2000"
        });

        var handler = new CnpjCapturingHttpHandler(HttpStatusCode.OK, json, url => capturedUrl = url);
        var service = BuildService(handler, token: "mytoken456");

        await service.ConsultarAsync("11222333000181");

        Assert.Contains("?token=mytoken456", capturedUrl);
    }
}

// ── Helpers ──────────────────────────────────────────────────────────────────

file sealed class FakeCnpjHttpHandler(HttpStatusCode status, string body) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}

file sealed class CnpjErrorHttpHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromException<HttpResponseMessage>(exception);
}

file sealed class CnpjCapturingHttpHandler(
    HttpStatusCode status,
    string body,
    Action<string> captureUrl) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        captureUrl(request.RequestUri?.ToString() ?? "");
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
