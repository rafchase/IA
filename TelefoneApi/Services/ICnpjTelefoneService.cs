using TelefoneApi.Models;

namespace TelefoneApi.Services;

public interface ICnpjTelefoneService
{
    Task<TelefoneCnpjResponse> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default);
}
