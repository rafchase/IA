using SerasaApi.Models;

namespace SerasaApi.Services;

public interface ICpfSerasaService
{
    Task<CpfSerasaResponse> ConsultarAsync(string cpf, CancellationToken cancellationToken = default);
}
