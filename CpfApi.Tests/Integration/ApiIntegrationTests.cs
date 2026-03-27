using System.Net;
using System.Text.Json;
using CpfApi.Models;
using CpfApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace CpfApi.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<ApiIntegrationTests.ApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── /health ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("ok", body);
    }

    // ── /consulta/cpf ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ConsultaCpf_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/consulta/cpf/529.982.247-25");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConsultaCpf_InvalidCpf_Returns400()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        var response = await _client.GetAsync("/consulta/cpf/000.000.000-00");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CPF inválido", body);
    }

    [Fact]
    public async Task ConsultaCpf_ValidCpf_Returns200()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        var response = await _client.GetAsync("/consulta/cpf/529.982.247-25");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CpfResponse>(body);
        Assert.NotNull(result);
        Assert.Equal("529.982.247-25", result.Cpf);
        Assert.Equal("NOME TESTE", result.Nome);
        Assert.Equal("REGULAR", result.Situacao);
    }

    [Fact]
    public async Task ConsultaCpf_NotFound_Returns404()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        // "404cpf" is the magic value the fake service uses to trigger NotFoundException
        var response = await _client.GetAsync("/consulta/cpf/111.444.777-35");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── /consulta/cnpj ────────────────────────────────────────────────────────

    [Fact]
    public async Task ConsultaCnpj_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/consulta/cnpj/11.222.333/0001-81");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConsultaCnpj_InvalidCnpj_Returns400()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        var response = await _client.GetAsync("/consulta/cnpj/00.000.000/0000-00");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CNPJ inválido", body);
    }

    [Fact]
    public async Task ConsultaCnpj_ValidCnpj_Returns200()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        var response = await _client.GetAsync("/consulta/cnpj/11.222.333/0001-81");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CnpjResponse>(body);
        Assert.NotNull(result);
        Assert.Equal("11.222.333/0001-81", result.Cnpj);
        Assert.Equal("EMPRESA TESTE LTDA", result.Nome);
        Assert.Equal("ATIVA", result.Situacao);
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    public class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiToken"] = "test-token"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // Replace real services with fakes so no HTTP calls go out
                services.RemoveAll<ICpfService>();
                services.AddSingleton<ICpfService, FakeCpfService>();

                services.RemoveAll<ICnpjService>();
                services.AddSingleton<ICnpjService, FakeCnpjService>();
            });
        }
    }
}

// ── Fake services ─────────────────────────────────────────────────────────────

file sealed class FakeCpfService : ICpfService
{
    public Task<CpfResponse> ConsultarAsync(string cpf, CancellationToken cancellationToken = default)
    {
        // Simulate not-found for a specific CPF
        if (cpf == "11144477735")
            throw new CpfNotFoundException(cpf);

        return Task.FromResult(new CpfResponse
        {
            Cpf = CpfApi.Validators.CpfValidator.Format(cpf),
            Nome = "NOME TESTE",
            Situacao = "REGULAR"
        });
    }
}

file sealed class FakeCnpjService : ICnpjService
{
    public Task<CnpjResponse> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CnpjResponse
        {
            Cnpj = CpfApi.Validators.CnpjValidator.Format(cnpj),
            Nome = "EMPRESA TESTE LTDA",
            Fantasia = "TESTE",
            Situacao = "ATIVA",
            Abertura = "01/01/2000"
        });
    }
}
