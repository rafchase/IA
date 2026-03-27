using System.Text.Json.Serialization;

namespace VeiculoApi.Models;

public class VeiculoResponse
{
    [JsonPropertyName("placa")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("marca")]
    public string Marca { get; set; } = string.Empty;

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("versao")]
    public string Versao { get; set; } = string.Empty;

    [JsonPropertyName("anoFabricacao")]
    public string AnoFabricacao { get; set; } = string.Empty;

    [JsonPropertyName("anoModelo")]
    public string AnoModelo { get; set; } = string.Empty;

    [JsonPropertyName("cor")]
    public string Cor { get; set; } = string.Empty;

    [JsonPropertyName("combustivel")]
    public string Combustivel { get; set; } = string.Empty;

    [JsonPropertyName("chassi")]
    public string Chassi { get; set; } = string.Empty;

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;

    [JsonPropertyName("fipe")]
    public List<FipeItem> Fipe { get; set; } = [];
}
