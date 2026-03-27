using System.Text.Json.Serialization;

namespace VeiculoApi.Models.External;

/// <summary>
/// Internal model — maps the apiplacas.com.br response.
/// GET https://www.apiplacas.com.br/?placa={placa}&token={token}
/// </summary>
public class ApiPlacasResponse
{
    [JsonPropertyName("MARCA")]
    public string Marca { get; set; } = string.Empty;

    [JsonPropertyName("MODELO")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("SUBMODELO")]
    public string Submodelo { get; set; } = string.Empty;

    [JsonPropertyName("VERSAO")]
    public string Versao { get; set; } = string.Empty;

    [JsonPropertyName("ano")]
    public string Ano { get; set; } = string.Empty;

    [JsonPropertyName("anoModelo")]
    public string AnoModelo { get; set; } = string.Empty;

    [JsonPropertyName("chassi")]
    public string Chassi { get; set; } = string.Empty;

    [JsonPropertyName("cor")]
    public string Cor { get; set; } = string.Empty;

    /// <summary>"0" = situação normal.</summary>
    [JsonPropertyName("codigoSituacao")]
    public string CodigoSituacao { get; set; } = string.Empty;

    [JsonPropertyName("extra")]
    public ApiPlacasExtra? Extra { get; set; }

    [JsonPropertyName("fipe")]
    public List<ApiPlacasFipe>? Fipe { get; set; }
}

public class ApiPlacasExtra
{
    [JsonPropertyName("ano_fabricacao")]
    public string AnoFabricacao { get; set; } = string.Empty;

    [JsonPropertyName("combustivel")]
    public string Combustivel { get; set; } = string.Empty;

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;
}

public class ApiPlacasFipe
{
    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("codigo_fipe")]
    public string CodigoFipe { get; set; } = string.Empty;

    [JsonPropertyName("marca")]
    public string Marca { get; set; } = string.Empty;

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("ano_modelo")]
    public int AnoModelo { get; set; }

    [JsonPropertyName("combustivel")]
    public string Combustivel { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    [JsonPropertyName("mes_referencia")]
    public string MesReferencia { get; set; } = string.Empty;
}
