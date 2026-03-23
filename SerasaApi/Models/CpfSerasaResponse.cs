using System.Text.Json.Serialization;

namespace SerasaApi.Models;

public class CpfSerasaResponse
{
    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Situação cadastral: REGULAR, NEGATIVADO, PROTESTADO, RESTRITO.</summary>
    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    /// <summary>Score Serasa de 0 a 1000.</summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>Faixa de risco: MUITO ALTO, ALTO, MÉDIO, BAIXO, MUITO BAIXO.</summary>
    [JsonPropertyName("faixaScore")]
    public string FaixaScore { get; set; } = string.Empty;

    [JsonPropertyName("totalPendencias")]
    public int TotalPendencias { get; set; }

    [JsonPropertyName("valorTotalPendencias")]
    public decimal ValorTotalPendencias { get; set; }
}
