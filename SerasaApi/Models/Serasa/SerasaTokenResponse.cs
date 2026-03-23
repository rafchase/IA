using System.Text.Json.Serialization;

namespace SerasaApi.Models.Serasa;

/// <summary>
/// Response from the Serasa Experian OAuth2 token endpoint.
/// POST /security/iam/v2/client-token
/// </summary>
public class SerasaTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>Lifetime in seconds (typically 3600).</summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
