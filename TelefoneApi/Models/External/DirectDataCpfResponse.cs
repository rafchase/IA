using System.Text.Json.Serialization;

namespace TelefoneApi.Models.External;

/// <summary>
/// Internal model — maps the Direct Data CadastroPessoaFisicaPlus API response.
/// GET https://apiv3.directd.com.br/api/CadastroPessoaFisicaPlus?CPF={cpf}&TOKEN={token}
/// </summary>
public class DirectDataCpfResponse
{
    [JsonPropertyName("metaDados")]
    public DirectDataMeta? MetaDados { get; set; }

    [JsonPropertyName("retorno")]
    public DirectDataRetorno? Retorno { get; set; }
}

public class DirectDataMeta
{
    [JsonPropertyName("consultaNome")]
    public string ConsultaNome { get; set; } = string.Empty;

    [JsonPropertyName("apiVersao")]
    public string ApiVersao { get; set; } = string.Empty;
}

public class DirectDataRetorno
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("telefones")]
    public List<DirectDataTelefone>? Telefones { get; set; }
}

public class DirectDataTelefone
{
    [JsonPropertyName("telefoneComDDD")]
    public string TelefoneComDDD { get; set; } = string.Empty;

    [JsonPropertyName("operadora")]
    public string Operadora { get; set; } = string.Empty;

    [JsonPropertyName("tipoTelefone")]
    public string TipoTelefone { get; set; } = string.Empty;

    [JsonPropertyName("telemarketingBloqueado")]
    public bool TelemarketingBloqueado { get; set; }

    [JsonPropertyName("whatsApp")]
    public bool WhatsApp { get; set; }
}
