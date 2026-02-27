using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimuladorMBM.Data;

namespace SimuladorMBM.Controllers;

/// <summary>
/// API de clientes — lista de clientes para o dropdown do Portal.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly MbmDbContext _db;

    public ClientesController(MbmDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista nomes dos clientes (para "Ver como cliente" no Portal).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get(CancellationToken cancellationToken = default)
    {
        var list = await _db.Clientes
            .AsNoTracking()
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => c.Nome)
            .ToListAsync(cancellationToken);
        return Ok(list);
    }
}
