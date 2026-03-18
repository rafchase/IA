using System.Text.Json.Serialization;

namespace CpfApi.Models;

public class ReceitaWsResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
