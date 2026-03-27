using System.Net;
using System.Text.Json;
using CpfApi.Models;
using CpfApi.Validators;

namespace CpfApi.Services;

public class CpfNotFoundException : Exception
{
    public CpfNotFoundException(string cpf)
        : base($"CPF {cpf} não encontrado") { }
}

public class CpfServiceUnavailableException : Exception
{
    public CpfServiceUnavailableException(string message)
        : base(message) { }
}

public class CpfService : ICpfService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CpfService> _logger;
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CpfService(HttpClient httpClient, ILogger<CpfService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<CpfResponse> ConsultarAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var digits = CpfValidator.Strip(cpf);
        var token = _configuration["ReceitaWsToken"];
        var url = string.IsNullOrWhiteSpace(token) ? digits : $"{digits}?token={token}";

        _logger.LogInformation("Consultando CPF {Cpf}", MaskCpf(digits));

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout ao consultar CPF");
            throw new CpfServiceUnavailableException("Timeout ao consultar a Receita Federal");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao consultar CPF");
            throw new CpfServiceUnavailableException("Serviço da Receita Federal indisponível");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new CpfNotFoundException(digits);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("ReceitaWS retornou status {Status}", response.StatusCode);
            throw new CpfServiceUnavailableException($"Serviço externo retornou status {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<ReceitaWsResponse>(content, JsonOptions);

        if (data is null || data.Status == "ERROR")
        {
            var msg = data?.Message ?? "Resposta inválida da Receita Federal";
            throw new CpfServiceUnavailableException(msg);
        }

        return new CpfResponse
        {
            Cpf = CpfValidator.Format(digits),
            Nome = data.Nome,
            Situacao = data.Situacao
        };
    }

    private static string MaskCpf(string digits)
    {
        if (digits.Length != 11) return "***";
        return $"{digits[..3]}.***.***.{digits[9..11]}";
    }
}
