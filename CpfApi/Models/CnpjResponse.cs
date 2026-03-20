using System.Text.Json.Serialization;

namespace CpfApi.Models;

public class CnpjResponse
{
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("fantasia")]
    public string Fantasia { get; set; } = string.Empty;

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("abertura")]
    public string Abertura { get; set; } = string.Empty;
}
