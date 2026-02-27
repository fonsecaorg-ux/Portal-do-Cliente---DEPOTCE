using Microsoft.EntityFrameworkCore;
using PortalCliente.Data;
using PortalCliente.Models;

namespace PortalCliente.Services;

/// <summary>
/// Origem dos dados: banco SQLite local (seed). Usado quando MBM:BaseUrl não está configurado.
/// </summary>
public class LocalOrigenDadosService : IOrigenDadosService
{
    private static readonly string[] StatusList = new[]
    {
        "Ag. Off Hire",
        "Ag. Envio Estimativa",
        "Ag. Limpeza",
        "Ag. Reparo",
        "Ag. Inspeção"
    };

    private readonly AppDbContext _context;

    public LocalOrigenDadosService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Container>> GetIsotanquesAsync(string? cliente, string? status, string? busca, CancellationToken cancellationToken = default)
    {
        var query = _context.Containers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(cliente))
            query = query.Where(c => c.Cliente == cliente);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(c => c.Codigo.Contains(busca) || c.Produto.Contains(busca));

        return await query.OrderBy(c => c.Codigo).ToListAsync(cancellationToken);
    }

    public async Task<Container?> GetIsotanquePorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await _context.Containers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Codigo == codigo.Trim(), cancellationToken);
    }

    public async Task<List<string>> GetClientesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Containers
            .AsNoTracking()
            .Select(c => c.Cliente)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetStatusListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(StatusList);
    }
}
