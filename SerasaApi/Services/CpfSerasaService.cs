using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SerasaApi.Models;
using SerasaApi.Models.Serasa;
using SerasaApi.Validators;

namespace SerasaApi.Services;

public class CpfSerasaNotFoundException : Exception
{
    public CpfSerasaNotFoundException(string cpf) : base($"CPF {cpf} não encontrado na Serasa") { }
}

public class CpfSerasaServiceUnavailableException : Exception
{
    public CpfSerasaServiceUnavailableException(string message) : base(message) { }
}

public class CpfSerasaService : ICpfSerasaService
{
    private readonly HttpClient _httpClient;
    private readonly ISerasaTokenService _tokenService;
    private readonly ILogger<CpfSerasaService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CpfSerasaService(
        HttpClient httpClient,
        ISerasaTokenService tokenService,
        ILogger<CpfSerasaService> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<CpfSerasaResponse> ConsultarAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var digits = CpfValidator.Strip(cpf);
        _logger.LogInformation("Consultando CPF {Cpf} na Serasa", MaskCpf(digits));

        string token;
        try
        {
            token = await _tokenService.GetAccessTokenAsync(cancellationToken);
        }
        catch (SerasaTokenServiceUnavailableException ex)
        {
            throw new CpfSerasaServiceUnavailableException($"Falha de autenticação Serasa: {ex.Message}");
        }

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = JsonSerializer.Serialize(new { document = digits, documentType = "CPF" });
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CpfSerasaServiceUnavailableException("Timeout na consulta de CPF na Serasa");
        }
        catch (HttpRequestException ex)
        {
            throw new CpfSerasaServiceUnavailableException(
                $"Erro de rede ao consultar CPF na Serasa: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new CpfSerasaNotFoundException(digits);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new CpfSerasaServiceUnavailableException("Token Serasa inválido ou expirado");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Serasa CPF retornou status {Status}", response.StatusCode);
            throw new CpfSerasaServiceUnavailableException(
                $"Serviço Serasa retornou status {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<SerasaCpfApiResponse>(content, JsonOptions);

        if (data is null || data.CodigoRetorno != "01")
        {
            var msg = data?.Mensagem ?? "Resposta inválida da Serasa";
            throw new CpfSerasaServiceUnavailableException(msg);
        }

        return new CpfSerasaResponse
        {
            Cpf = CpfValidator.Format(digits),
            Nome = data.Nome,
            Situacao = data.Situacao,
            Score = data.Score?.Pontos ?? 0,
            FaixaScore = data.Score?.Faixa ?? string.Empty,
            TotalPendencias = data.Pendencias?.Quantidade ?? 0,
            ValorTotalPendencias = data.Pendencias?.ValorTotal ?? 0m
        };
    }

    private static string MaskCpf(string d) =>
        d.Length == 11 ? $"{d[..3]}.***.***.{d[9..11]}" : "***";
}
