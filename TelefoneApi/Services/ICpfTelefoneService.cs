using TelefoneApi.Models;

namespace TelefoneApi.Services;

public interface ICpfTelefoneService
{
    Task<TelefoneCpfResponse> ConsultarAsync(string cpf, CancellationToken cancellationToken = default);
}
