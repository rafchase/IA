using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SerasaApi.Models.Serasa;

namespace SerasaApi.Services;

public class SerasaTokenServiceUnavailableException : Exception
{
    public SerasaTokenServiceUnavailableException(string message) : base(message) { }
}

/// <summary>
/// Obtains and caches an OAuth2 client_credentials token from Serasa Experian.
/// Registered as singleton so the token is shared across requests.
/// </summary>
public class SerasaTokenService : ISerasaTokenService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SerasaTokenService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SerasaTokenService(
        IHttpClientFactory httpClientFactory,
        ILogger<SerasaTokenService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("SerasaToken");
        _logger = logger;
        _clientId = configuration["SerasaClientId"] ?? string.Empty;
        _clientSecret = configuration["SerasaClientSecret"] ?? string.Empty;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Fast path: token still valid (with 30s buffer)
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-checked locking
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            _logger.LogInformation("Obtendo token OAuth2 Serasa Experian");

            using var request = new HttpRequestMessage(HttpMethod.Post, "");

            // Basic auth: base64(clientId:clientSecret)
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            });

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new SerasaTokenServiceUnavailableException("Timeout ao obter token Serasa");
            }
            catch (HttpRequestException ex)
            {
                throw new SerasaTokenServiceUnavailableException(
                    $"Erro de rede ao obter token Serasa: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Serasa token endpoint retornou {Status}", response.StatusCode);
                throw new SerasaTokenServiceUnavailableException(
                    $"Falha na autenticação Serasa: status {(int)response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<SerasaTokenResponse>(content, JsonOptions);

            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new SerasaTokenServiceUnavailableException("Token inválido retornado pela Serasa");

            _cachedToken = tokenResponse.AccessToken;
            // Buffer 30 seconds to avoid edge-case expiry
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30);

            _logger.LogInformation("Token Serasa obtido, expira em {Expiry}", _tokenExpiry);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}
