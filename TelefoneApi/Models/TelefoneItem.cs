using System.Text.Json.Serialization;

namespace TelefoneApi.Models;

public class TelefoneItem
{
    [JsonPropertyName("numero")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("operadora")]
    public string Operadora { get; set; } = string.Empty;

    /// <summary>CELULAR ou FIXO.</summary>
    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonPropertyName("whatsApp")]
    public bool WhatsApp { get; set; }
}
