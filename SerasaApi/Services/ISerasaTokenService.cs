namespace SerasaApi.Services;

public interface ISerasaTokenService
{
    /// <summary>Returns a valid Bearer access token, refreshing if expired.</summary>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
