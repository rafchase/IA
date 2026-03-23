using System.Text.Json.Serialization;

namespace TelefoneApi.Models;

public class TelefoneCnpjResponse
{
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("razaoSocial")]
    public string RazaoSocial { get; set; } = string.Empty;

    [JsonPropertyName("telefones")]
    public List<TelefoneItem> Telefones { get; set; } = [];
}
