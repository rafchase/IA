using SerasaApi.Models;

namespace SerasaApi.Services;

public interface ICnpjSerasaService
{
    Task<CnpjSerasaResponse> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default);
}
