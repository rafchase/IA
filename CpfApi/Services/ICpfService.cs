using CpfApi.Models;

namespace CpfApi.Services;

public interface ICpfService
{
    Task<CpfResponse> ConsultarAsync(string cpf, CancellationToken cancellationToken = default);
}
