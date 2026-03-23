using System.Text.Json.Serialization;

namespace SerasaApi.Models.Serasa;

/// <summary>
/// Internal model — maps the Serasa Experian CPF bureau API response.
/// </summary>
public class SerasaCpfApiResponse
{
    /// <summary>"01" = success.</summary>
    [JsonPropertyName("codigoRetorno")]
    public string CodigoRetorno { get; set; } = string.Empty;

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>e.g. "REGULAR", "NEGATIVADO", "PROTESTADO", "RESTRITO"</summary>
    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public SerasaScoreBlock? Score { get; set; }

    [JsonPropertyName("pendencias")]
    public SerasaOcorrencias? Pendencias { get; set; }
}

public class SerasaScoreBlock
{
    [JsonPropertyName("pontos")]
    public int Pontos { get; set; }

    /// <summary>e.g. "MUITO ALTO", "ALTO", "MÉDIO", "BAIXO", "MUITO BAIXO"</summary>
    [JsonPropertyName("faixa")]
    public string Faixa { get; set; } = string.Empty;
}

public class SerasaOcorrencias
{
    [JsonPropertyName("quantidade")]
    public int Quantidade { get; set; }

    [JsonPropertyName("valorTotal")]
    public decimal ValorTotal { get; set; }
}
