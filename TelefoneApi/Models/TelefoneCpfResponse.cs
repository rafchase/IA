using System.Text.Json.Serialization;

namespace TelefoneApi.Models;

public class TelefoneCpfResponse
{
    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("telefones")]
    public List<TelefoneItem> Telefones { get; set; } = [];
}
