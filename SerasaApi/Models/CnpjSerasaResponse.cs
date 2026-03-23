using System.Text.Json.Serialization;

namespace SerasaApi.Models;

public class CnpjSerasaResponse
{
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("razaoSocial")]
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>Situação da empresa: REGULAR, NEGATIVADA, RESTRITA.</summary>
    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    /// <summary>Score PJ Serasa de 0 a 1000.</summary>
    [JsonPropertyName("scorePJ")]
    public int ScorePJ { get; set; }

    [JsonPropertyName("faixaScorePJ")]
    public string FaixaScorePJ { get; set; } = string.Empty;

    [JsonPropertyName("totalPendencias")]
    public int TotalPendencias { get; set; }

    [JsonPropertyName("valorTotalPendencias")]
    public decimal ValorTotalPendencias { get; set; }

    [JsonPropertyName("totalProtestos")]
    public int TotalProtestos { get; set; }

    [JsonPropertyName("valorTotalProtestos")]
    public decimal ValorTotalProtestos { get; set; }
}
