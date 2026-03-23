using System.Net;
using System.Text.Json;
using TelefoneApi.Models;
using TelefoneApi.Models.External;
using TelefoneApi.Validators;

namespace TelefoneApi.Services;

public class CpfTelefoneNotFoundException : Exception
{
    public CpfTelefoneNotFoundException(string cpf) : base($"CPF {cpf} não encontrado na Direct Data") { }
}

public class CpfTelefoneServiceUnavailableException : Exception
{
    public CpfTelefoneServiceUnavailableException(string message) : base(message) { }
}

public class CpfTelefoneService : ICpfTelefoneService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CpfTelefoneService> _logger;
    private readonly string _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CpfTelefoneService(
        HttpClient httpClient,
        ILogger<CpfTelefoneService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _token = configuration["DirectDataToken"] ?? string.Empty;
    }

    public async Task<TelefoneCpfResponse> ConsultarAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var digits = CpfValidator.Strip(cpf);
        _logger.LogInformation("Consultando telefones do CPF {Cpf}", MaskCpf(digits));

        var url = $"api/CadastroPessoaFisicaPlus?CPF={digits}&TOKEN={_token}";

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CpfTelefoneServiceUnavailableException("Timeout ao consultar Direct Data");
        }
        catch (HttpRequestException ex)
        {
            throw new CpfTelefoneServiceUnavailableException(
                $"Erro de rede ao consultar Direct Data: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new CpfTelefoneNotFoundException(digits);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new CpfTelefoneServiceUnavailableException(
                "Token Direct Data inválido ou ausente. Configure DirectDataToken.");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Direct Data retornou status {Status}", response.StatusCode);
            throw new CpfTelefoneServiceUnavailableException(
                $"Serviço externo retornou status {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<DirectDataCpfResponse>(content, JsonOptions);

        if (data?.Retorno is null)
            throw new CpfTelefoneServiceUnavailableException("Resposta inválida da Direct Data");

        var telefones = (data.Retorno.Telefones ?? [])
            .Select(t => new TelefoneItem
            {
                Numero = t.TelefoneComDDD,
                Operadora = t.Operadora,
                Tipo = t.TipoTelefone,
                WhatsApp = t.WhatsApp
            })
            .ToList();

        return new TelefoneCpfResponse
        {
            Cpf = CpfValidator.Format(digits),
            Nome = data.Retorno.Name,
            Telefones = telefones
        };
    }

    private static string MaskCpf(string d) =>
        d.Length == 11 ? $"{d[..3]}.***.***.{d[9..11]}" : "***";
}
