using System.Net;
using System.Text.Json;
using VeiculoApi.Models;
using VeiculoApi.Models.External;
using VeiculoApi.Validators;

namespace VeiculoApi.Services;

public class VeiculoNotFoundException : Exception
{
    public VeiculoNotFoundException(string placa) : base($"Placa {placa} não encontrada") { }
}

public class VeiculoServiceUnavailableException : Exception
{
    public VeiculoServiceUnavailableException(string message) : base(message) { }
}

public class VeiculoService : IVeiculoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VeiculoService> _logger;
    private readonly string _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public VeiculoService(
        HttpClient httpClient,
        ILogger<VeiculoService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _token = configuration["ApiPlacasToken"] ?? string.Empty;
    }

    public async Task<VeiculoResponse> ConsultarAsync(string placa, CancellationToken cancellationToken = default)
    {
        var stripped = PlacaValidator.Strip(placa);
        _logger.LogInformation("Consultando placa {Placa}", MaskPlaca(stripped));

        var url = $"?placa={stripped}&token={_token}";

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new VeiculoServiceUnavailableException("Timeout ao consultar apiplacas.com.br");
        }
        catch (HttpRequestException ex)
        {
            throw new VeiculoServiceUnavailableException(
                $"Erro de rede ao consultar apiplacas.com.br: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new VeiculoNotFoundException(stripped);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new VeiculoServiceUnavailableException(
                "Token apiplacas inválido ou ausente. Configure ApiPlacasToken.");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("apiplacas retornou status {Status}", response.StatusCode);
            throw new VeiculoServiceUnavailableException(
                $"Serviço externo retornou status {(int)response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<ApiPlacasResponse>(content, JsonOptions);

        if (data is null || string.IsNullOrWhiteSpace(data.Marca))
            throw new VeiculoNotFoundException(stripped);

        var fipe = (data.Fipe ?? [])
            .Select(f => new FipeItem
            {
                CodigoFipe = f.CodigoFipe,
                Marca = f.Marca,
                Modelo = f.Modelo,
                AnoModelo = f.AnoModelo,
                Combustivel = f.Combustivel,
                Valor = f.Valor,
                MesReferencia = f.MesReferencia,
                Score = f.Score
            })
            .OrderByDescending(f => f.Score)
            .ToList();

        return new VeiculoResponse
        {
            Placa = PlacaValidator.Format(stripped),
            Marca = data.Marca,
            Modelo = data.Modelo,
            Versao = data.Versao,
            AnoFabricacao = data.Extra?.AnoFabricacao ?? data.Ano,
            AnoModelo = data.AnoModelo,
            Cor = data.Cor,
            Combustivel = data.Extra?.Combustivel ?? string.Empty,
            Chassi = data.Chassi,
            Municipio = data.Extra?.Municipio ?? string.Empty,
            Uf = data.Extra?.Uf ?? string.Empty,
            Fipe = fipe
        };
    }

    private static string MaskPlaca(string p) =>
        p.Length == 7 ? $"{p[..3]}-****" : "***";
}
