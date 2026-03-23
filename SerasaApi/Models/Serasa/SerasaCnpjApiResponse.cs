using System.Text.Json.Serialization;

namespace SerasaApi.Models.Serasa;

/// <summary>
/// Internal model — maps the Serasa Experian CNPJ bureau API response.
/// </summary>
public class SerasaCnpjApiResponse
{
    [JsonPropertyName("codigoRetorno")]
    public string CodigoRetorno { get; set; } = string.Empty;

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("razaoSocial")]
    public string RazaoSocial { get; set; } = string.Empty;

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public SerasaScoreBlock? Score { get; set; }

    [JsonPropertyName("pendencias")]
    public SerasaOcorrencias? Pendencias { get; set; }

    [JsonPropertyName("protestos")]
    public SerasaOcorrencias? Protestos { get; set; }
}
