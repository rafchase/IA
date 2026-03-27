using VeiculoApi.Models;

namespace VeiculoApi.Services;

public interface IVeiculoService
{
    Task<VeiculoResponse> ConsultarAsync(string placa, CancellationToken cancellationToken = default);
}
