using PortalCliente.Models;

namespace PortalCliente.Services;

/// <summary>
/// Origem dos dados do portal: banco local (desenvolvimento) ou API do MBM.
/// </summary>
public interface IOrigenDadosService
{
    Task<List<Container>> GetIsotanquesAsync(string? cliente, string? status, string? busca, CancellationToken cancellationToken = default);
    Task<Container?> GetIsotanquePorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<List<string>> GetClientesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetStatusListAsync(CancellationToken cancellationToken = default);
}
