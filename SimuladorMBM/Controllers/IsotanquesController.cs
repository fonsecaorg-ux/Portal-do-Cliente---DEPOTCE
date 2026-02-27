using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimuladorMBM.Data;
using SimuladorMBM.Models;

namespace SimuladorMBM.Controllers;

/// <summary>
/// API de isotanques — simula o que o MBM expõe para o Portal do Cliente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IsotanquesController : ControllerBase
{
    private readonly MbmDbContext _db;

    public IsotanquesController(MbmDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista isotanques com filtros opcionais (cliente, status, busca).
    /// Retorna DTO compatível com o modelo do Portal.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IsotanqueDto>>> Get(
        [FromQuery] string? cliente,
        [FromQuery] string? status,
        [FromQuery] string? busca,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Isotanques
            .AsNoTracking()
            .Include(i => i.Cliente)
            .Include(i => i.Produto)
            .Where(i => i.Ativo);

        if (!string.IsNullOrWhiteSpace(cliente))
            query = query.Where(i => i.Cliente.Nome == cliente);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(i => i.Codigo.Contains(busca) || i.Produto.Nome.Contains(busca));

        var hoje = DateTime.Today;
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var raw = await query
            .OrderBy(i => i.Codigo)
            .Select(i => new
            {
                i.Id,
                i.Codigo,
                Produto = i.Produto.Nome,
                Cliente = i.Cliente.Nome,
                i.Status,
                i.DataInicioStatus,
                i.PrevisaoLiberacao,
                i.DataHoraDescarregadoPatio,
                i.DataHoraCarregadoVeiculo,
                i.DataSaida,
                i.PrevisaoChegadaTerminal,
                i.UrlFoto,
                i.UrlLaudoVistoria,
                i.UrlCertificadoLavagem,
                i.NumeroBooking
            })
            .ToListAsync(cancellationToken);

        var list = raw.Select(i => new IsotanqueDto
        {
            Id = i.Id,
            Codigo = i.Codigo,
            Produto = i.Produto,
            Cliente = i.Cliente,
            Status = i.Status,
            DiasNoStatus = i.DataInicioStatus.HasValue ? (int)(hoje - i.DataInicioStatus.Value.Date).TotalDays : null,
            PrevisaoLiberacao = i.PrevisaoLiberacao,
            DataHoraDescarregadoPatio = i.DataHoraDescarregadoPatio,
            DataHoraCarregadoVeiculo = i.DataHoraCarregadoVeiculo,
            DataSaida = i.DataSaida,
            PrevisaoChegadaTerminal = i.PrevisaoChegadaTerminal,
            UrlFoto = string.IsNullOrEmpty(i.UrlFoto) ? null : (i.UrlFoto!.StartsWith("http") ? i.UrlFoto : baseUrl + (i.UrlFoto.StartsWith("/") ? i.UrlFoto : "/" + i.UrlFoto)),
            UrlLaudoVistoria = string.IsNullOrEmpty(i.UrlLaudoVistoria) ? null : (i.UrlLaudoVistoria!.StartsWith("http") ? i.UrlLaudoVistoria : baseUrl + (i.UrlLaudoVistoria.StartsWith("/") ? i.UrlLaudoVistoria : "/" + i.UrlLaudoVistoria)),
            UrlCertificadoLavagem = string.IsNullOrEmpty(i.UrlCertificadoLavagem) ? null : (i.UrlCertificadoLavagem!.StartsWith("http") ? i.UrlCertificadoLavagem : baseUrl + (i.UrlCertificadoLavagem.StartsWith("/") ? i.UrlCertificadoLavagem : "/" + i.UrlCertificadoLavagem)),
            NumeroBooking = i.NumeroBooking
        }).ToList();

        return Ok(list);
    }

    /// <summary>
    /// Busca isotanque por código.
    /// </summary>
    [HttpGet("{codigo}")]
    public async Task<ActionResult<IsotanqueDto>> GetPorCodigo(string codigo, CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.Today;
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var raw = await _db.Isotanques
            .AsNoTracking()
            .Include(i => i.Cliente)
            .Include(i => i.Produto)
            .Where(i => i.Ativo && i.Codigo == codigo)
            .Select(i => new
            {
                i.Id,
                i.Codigo,
                Produto = i.Produto.Nome,
                Cliente = i.Cliente.Nome,
                i.Status,
                i.DataInicioStatus,
                i.PrevisaoLiberacao,
                i.DataHoraDescarregadoPatio,
                i.DataHoraCarregadoVeiculo,
                i.DataSaida,
                i.PrevisaoChegadaTerminal,
                i.UrlFoto,
                i.UrlLaudoVistoria,
                i.UrlCertificadoLavagem,
                i.NumeroBooking
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (raw == null)
            return NotFound();

        var item = new IsotanqueDto
        {
            Id = raw.Id,
            Codigo = raw.Codigo,
            Produto = raw.Produto,
            Cliente = raw.Cliente,
            Status = raw.Status,
            DiasNoStatus = raw.DataInicioStatus.HasValue ? (int)(hoje - raw.DataInicioStatus.Value.Date).TotalDays : null,
            PrevisaoLiberacao = raw.PrevisaoLiberacao,
            DataHoraDescarregadoPatio = raw.DataHoraDescarregadoPatio,
            DataHoraCarregadoVeiculo = raw.DataHoraCarregadoVeiculo,
            DataSaida = raw.DataSaida,
            PrevisaoChegadaTerminal = raw.PrevisaoChegadaTerminal,
            UrlFoto = string.IsNullOrEmpty(raw.UrlFoto) ? null : (raw.UrlFoto!.StartsWith("http") ? raw.UrlFoto : baseUrl + (raw.UrlFoto.StartsWith("/") ? raw.UrlFoto : "/" + raw.UrlFoto)),
            UrlLaudoVistoria = string.IsNullOrEmpty(raw.UrlLaudoVistoria) ? null : (raw.UrlLaudoVistoria!.StartsWith("http") ? raw.UrlLaudoVistoria : baseUrl + (raw.UrlLaudoVistoria.StartsWith("/") ? raw.UrlLaudoVistoria : "/" + raw.UrlLaudoVistoria)),
            UrlCertificadoLavagem = string.IsNullOrEmpty(raw.UrlCertificadoLavagem) ? null : (raw.UrlCertificadoLavagem!.StartsWith("http") ? raw.UrlCertificadoLavagem : baseUrl + (raw.UrlCertificadoLavagem.StartsWith("/") ? raw.UrlCertificadoLavagem : "/" + raw.UrlCertificadoLavagem)),
            NumeroBooking = raw.NumeroBooking
        };

        return Ok(item);
    }
}