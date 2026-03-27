using System.Net;
using System.Text;
using System.Text.Json;
using CpfApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CpfApi.Tests.Services;

public class CpfServiceTests
{
    private static CpfService BuildService(
        HttpMessageHandler handler,
        string? token = null,
        string baseAddress = "http://fake-receita-ws/v1/cpf/")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(token is not null
                ? new Dictionary<string, string?> { ["ReceitaWsToken"] = token }
                : new Dictionary<string, string?>())
            .Build();

        return new CpfService(httpClient, NullLogger<CpfService>.Instance, config);
    }

    [Fact]
    public async Task ConsultarAsync_SuccessResponse_ReturnsCpfResponse()
    {
        var json = JsonSerializer.Serialize(new
        {
            status = "OK",
            nome = "FULANO DE TAL",
            situacao = "REGULAR",
            cpf = "52998224725"
        });

        var handler = new FakeHttpHandler(HttpStatusCode.OK, json);
        var service = BuildService(handler);

        var result = await service.ConsultarAsync("52998224725");

        Assert.Equal("529.982.247-25", result.Cpf);
        Assert.Equal("FULANO DE TAL", result.Nome);
        Assert.Equal("REGULAR", result.Situacao);
    }

    [Fact]
    public async Task ConsultarAsync_NotFound_ThrowsCpfNotFoundException()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.NotFound, "");
        var service = BuildService(handler);

        await Assert.ThrowsAsync<CpfNotFoundException>(
            () => service.ConsultarAsync("52998224725"));
    }

    [Fact]
    public async Task ConsultarAsync_ServerError_ThrowsCpfServiceUnavailableException()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.InternalServerError, "");
        var service = BuildService(handler);

        await Assert.ThrowsAsync<CpfServiceUnavailableException>(
            () => service.ConsultarAsync("52998224725"));
    }

    [Fact]
    public async Task ConsultarAsync_ErrorStatusInBody_ThrowsCpfServiceUnavailableException()
    {
        var json = JsonSerializer.Serialize(new
        {
            status = "ERROR",
            message = "CPF não encontrado na base"
        });

        var handler = new FakeHttpHandler(HttpStatusCode.OK, json);
        var service = BuildService(handler);

        var ex = await Assert.ThrowsAsync<CpfServiceUnavailableException>(
            () => service.ConsultarAsync("52998224725"));

        Assert.Equal("CPF não encontrado na base", ex.Message);
    }

    [Fact]
    public async Task ConsultarAsync_NetworkError_ThrowsCpfServiceUnavailableException()
    {
        var handler = new ErrorHttpHandler(new HttpRequestException("connection refused"));
        var service = BuildService(handler);

        await Assert.ThrowsAsync<CpfServiceUnavailableException>(
            () => service.ConsultarAsync("52998224725"));
    }

    [Fact]
    public async Task ConsultarAsync_WithToken_AppendsTokenToUrl()
    {
        var capturedUrl = "";
        var json = JsonSerializer.Serialize(new
        {
            status = "OK",
            nome = "FULANO",
            situacao = "REGULAR",
            cpf = "52998224725"
        });

        var handler = new CapturingHttpHandler(HttpStatusCode.OK, json, url => capturedUrl = url);
        var service = BuildService(handler, token: "mytoken123");

        await service.ConsultarAsync("52998224725");

        Assert.Contains("?token=mytoken123", capturedUrl);
    }

    [Fact]
    public async Task ConsultarAsync_WithoutToken_DoesNotAppendToken()
    {
        var capturedUrl = "";
        var json = JsonSerializer.Serialize(new
        {
            status = "OK",
            nome = "FULANO",
            situacao = "REGULAR",
            cpf = "52998224725"
        });

        var handler = new CapturingHttpHandler(HttpStatusCode.OK, json, url => capturedUrl = url);
        var service = BuildService(handler);

        await service.ConsultarAsync("52998224725");

        Assert.DoesNotContain("token", capturedUrl);
    }
}

// ── Helpers ──────────────────────────────────────────────────────────────────

file sealed class FakeHttpHandler(HttpStatusCode status, string body) : HttpMessageHandler
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

file sealed class ErrorHttpHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromException<HttpResponseMessage>(exception);
}

file sealed class CapturingHttpHandler(
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
