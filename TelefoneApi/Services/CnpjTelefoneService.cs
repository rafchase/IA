using System.Net;
using System.Text.Json;
using TelefoneApi.Models;
using TelefoneApi.Models.External;
using TelefoneApi.Validators;

namespace TelefoneApi.Services;

public class CnpjTelefoneNotFoundException : Exception
{
    public CnpjTelefoneNotFoundException(string cnpj) : base($"CNPJ {cnpj} não encontrado na BrasilAPI") { }
}

public class CnpjTelefoneServiceUnavailableException : Exception
{
    public CnpjTelefoneServiceUnavailableException(string message) : base(message) { }
}

public class CnpjTelefoneService : ICnpjTelefoneService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CnpjTelefoneService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CnpjTelefoneService(HttpClient httpClient, ILogger<CnpjTelefoneService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TelefoneCnpjResponse> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        var digits = CnpjValidator.Strip(cnpj);
        _logger.LogInformation("Consultando telefones do CNPJ {Cnpj}", MaskCnpj(digits));

        var url = $"api/cnpj/v1/{digits}";

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CnpjTelefoneServiceUnavailableException("Timeout ao consultar BrasilAPI");
        }
        catch (HttpRequestException ex)
        {
            throw new CnpjTelefoneServiceUnavailableException(
                $"Erro de rede ao consultar BrasilAPI: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new CnpjTelefoneNotFoundException(digits);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("BrasilAPI retornou status {Status}", response.StatusCode);
            throw new CnpjTelefoneServiceUnavailableException(
                $"Serviço externo retornou status {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(content, JsonOptions);

        if (data is null)
            throw new CnpjTelefoneServiceUnavailableException("Resposta inválida da BrasilAPI");

        var telefones = new List<TelefoneItem>();

        if (!string.IsNullOrWhiteSpace(data.DddTelefone1))
            telefones.Add(new TelefoneItem { Numero = data.DddTelefone1.Trim(), Tipo = "FIXO" });

        if (!string.IsNullOrWhiteSpace(data.DddTelefone2))
            telefones.Add(new TelefoneItem { Numero = data.DddTelefone2.Trim(), Tipo = "FIXO" });

        return new TelefoneCnpjResponse
        {
            Cnpj = CnpjValidator.Format(digits),
            RazaoSocial = data.RazaoSocial,
            Telefones = telefones
        };
    }

    private static string MaskCnpj(string d) =>
        d.Length == 14 ? $"{d[..2]}.***.***/****-{d[12..14]}" : "***";
}
