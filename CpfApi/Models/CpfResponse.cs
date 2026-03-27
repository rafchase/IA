using System.Text.Json.Serialization;

namespace CpfApi.Models;

public class CpfResponse
{
    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;
}
