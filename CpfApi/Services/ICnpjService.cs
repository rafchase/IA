using CpfApi.Models;

namespace CpfApi.Services;

public interface ICnpjService
{
    Task<CnpjResponse> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default);
}
