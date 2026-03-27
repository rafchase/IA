using System.Text.Json.Serialization;

namespace VeiculoApi.Models;

public class FipeItem
{
    [JsonPropertyName("codigoFipe")]
    public string CodigoFipe { get; set; } = string.Empty;

    [JsonPropertyName("marca")]
    public string Marca { get; set; } = string.Empty;

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("anoModelo")]
    public int AnoModelo { get; set; }

    [JsonPropertyName("combustivel")]
    public string Combustivel { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    [JsonPropertyName("mesReferencia")]
    public string MesReferencia { get; set; } = string.Empty;

    /// <summary>Score de correspondência (0-100). Maior = melhor match.</summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }
}
