using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SerasaApi.Models;
using SerasaApi.Models.Serasa;
using SerasaApi.Validators;

namespace SerasaApi.Services;

public class CnpjSerasaNotFoundException : Exception
{
    public CnpjSerasaNotFoundException(string cnpj) : base($"CNPJ {cnpj} não encontrado na Serasa") { }
}

public class CnpjSerasaServiceUnavailableException : Exception
{
    public CnpjSerasaServiceUnavailableException(string message) : base(message) { }
}

public class CnpjSerasaService : ICnpjSerasaService
{
    private readonly HttpClient _httpClient;
    private readonly ISerasaTokenService _tokenService;
    private readonly ILogger<CnpjSerasaService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CnpjSerasaService(
        HttpClient httpClient,
        ISerasaTokenService tokenService,
        ILogger<CnpjSerasaService> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<CnpjSerasaResponse> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        var digits = CnpjValidator.Strip(cnpj);
        _logger.LogInformation("Consultando CNPJ {Cnpj} na Serasa", MaskCnpj(digits));

        string token;
        try
        {
            token = await _tokenService.GetAccessTokenAsync(cancellationToken);
        }
        catch (SerasaTokenServiceUnavailableException ex)
        {
            throw new CnpjSerasaServiceUnavailableException($"Falha de autenticação Serasa: {ex.Message}");
        }

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = JsonSerializer.Serialize(new { document = digits, documentType = "CNPJ" });
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CnpjSerasaServiceUnavailableException("Timeout na consulta de CNPJ na Serasa");
        }
        catch (HttpRequestException ex)
        {
            throw new CnpjSerasaServiceUnavailableException(
                $"Erro de rede ao consultar CNPJ na Serasa: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new CnpjSerasaNotFoundException(digits);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new CnpjSerasaServiceUnavailableException("Token Serasa inválido ou expirado");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Serasa CNPJ retornou status {Status}", response.StatusCode);
            throw new CnpjSerasaServiceUnavailableException(
                $"Serviço Serasa retornou status {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<SerasaCnpjApiResponse>(content, JsonOptions);

        if (data is null || data.CodigoRetorno != "01")
        {
            var msg = data?.Mensagem ?? "Resposta inválida da Serasa";
            throw new CnpjSerasaServiceUnavailableException(msg);
        }

        return new CnpjSerasaResponse
        {
            Cnpj = CnpjValidator.Format(digits),
            RazaoSocial = data.RazaoSocial,
            Situacao = data.Situacao,
            ScorePJ = data.Score?.Pontos ?? 0,
            FaixaScorePJ = data.Score?.Faixa ?? string.Empty,
            TotalPendencias = data.Pendencias?.Quantidade ?? 0,
            ValorTotalPendencias = data.Pendencias?.ValorTotal ?? 0m,
            TotalProtestos = data.Protestos?.Quantidade ?? 0,
            ValorTotalProtestos = data.Protestos?.ValorTotal ?? 0m
        };
    }

    private static string MaskCnpj(string d) =>
        d.Length == 14 ? $"{d[..2]}.***.***/****-{d[12..14]}" : "***";
}
