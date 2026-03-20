using System.Net;
using System.Text.Json;
using CpfApi.Models;
using CpfApi.Validators;

namespace CpfApi.Services;

public class CnpjNotFoundException : Exception
{
    public CnpjNotFoundException(string cnpj)
        : base($"CNPJ {cnpj} não encontrado") { }
}

public class CnpjServiceUnavailableException : Exception
{
    public CnpjServiceUnavailableException(string message)
        : base(message) { }
}

public class CnpjService : ICnpjService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CnpjService> _logger;
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CnpjService(HttpClient httpClient, ILogger<CnpjService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<CnpjResponse> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        var digits = CnpjValidator.Strip(cnpj);
        var token = _configuration["ReceitaWsCnpjToken"];
        var url = string.IsNullOrWhiteSpace(token) ? digits : $"{digits}?token={token}";

        _logger.LogInformation("Consultando CNPJ {Cnpj}", MaskCnpj(digits));

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout ao consultar CNPJ");
            throw new CnpjServiceUnavailableException("Timeout ao consultar a Receita Federal");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao consultar CNPJ");
            throw new CnpjServiceUnavailableException("Serviço da Receita Federal indisponível");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new CnpjNotFoundException(digits);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("ReceitaWS retornou status {Status}", response.StatusCode);
            throw new CnpjServiceUnavailableException($"Serviço externo retornou status {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<ReceitaWsCnpjResponse>(content, JsonOptions);

        if (data is null || data.Status == "ERROR")
        {
            var msg = data?.Message ?? "Resposta inválida da Receita Federal";
            throw new CnpjServiceUnavailableException(msg);
        }

        return new CnpjResponse
        {
            Cnpj = CnpjValidator.Format(digits),
            Nome = data.Nome,
            Fantasia = data.Fantasia,
            Situacao = data.Situacao,
            Abertura = data.Abertura
        };
    }

    private static string MaskCnpj(string digits)
    {
        if (digits.Length != 14) return "***";
        return $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/****-**";
    }
}
