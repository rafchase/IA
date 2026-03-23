using System.Text.Json.Serialization;

namespace TelefoneApi.Models.External;

/// <summary>
/// Internal model — maps the BrasilAPI CNPJ response.
/// GET https://brasilapi.com.br/api/cnpj/v1/{cnpj}
/// </summary>
public class BrasilApiCnpjResponse
{
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("razao_social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [JsonPropertyName("nome_fantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    /// <summary>e.g. "11 3500-6000" — may be null or empty.</summary>
    [JsonPropertyName("ddd_telefone_1")]
    public string? DddTelefone1 { get; set; }

    [JsonPropertyName("ddd_telefone_2")]
    public string? DddTelefone2 { get; set; }

    [JsonPropertyName("descricao_situacao_cadastral")]
    public string SituacaoCadastral { get; set; } = string.Empty;
}
