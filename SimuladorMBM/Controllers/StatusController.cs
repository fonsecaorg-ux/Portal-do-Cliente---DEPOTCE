using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimuladorMBM.Data;

namespace SimuladorMBM.Controllers;

/// <summary>
/// API de status de isotanque — lista de status para filtro no Portal.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly MbmDbContext _db;

    public StatusController(MbmDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista descrições dos status (Ag. Limpeza, Ag. Inspeção, etc.).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get(CancellationToken cancellationToken = default)
    {
        var list = await _db.StatusIsotanques
            .AsNoTracking()
            .OrderBy(s => s.Ordem)
            .Select(s => s.Descricao)
            .ToListAsync(cancellationToken);
        return Ok(list);
    }
}
