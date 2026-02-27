using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalCliente.Data;
using PortalCliente.Models;
using PortalCliente.Services;

namespace PortalCliente.Controllers;

[Authorize] // toda a home exige login
public class HomeController : Controller
{
    private readonly IOrigenDadosService _origen;
    private readonly IConfiguration _configuration;
    private readonly UserManager<UsuarioAplicacao> _userManager;
    private readonly IConfiguracaoContatoService _configContato;
    private readonly AppDbContext _db;

    public HomeController(
        IOrigenDadosService origen,
        IConfiguration configuration,
        UserManager<UsuarioAplicacao> userManager,
        IConfiguracaoContatoService configContato,
        AppDbContext db)
    {
        _origen = origen;
        _configuration = configuration;
        _userManager = userManager;
        _configContato = configContato;
        _db = db;
    }

    private const int PaginaPadrao = 1;
    private const int ItensPorPaginaPadrao = 10;

    public async Task<IActionResult> Index(
        string? busca,
        string? cliente,
        string? status,
        int? proximos,
        string? ordenarPor = "Codigo",
        bool ordemAsc = true,
        int pagina = PaginaPadrao,
        int itensPorPagina = ItensPorPaginaPadrao)
    {
        itensPorPagina = Math.Clamp(itensPorPagina, 5, 100);
        pagina = Math.Max(1, pagina);

        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");

        // Cliente comum: sempre filtra pelo próprio cliente, ignora parâmetro da querystring
        if (!ehAdmin)
            cliente = usuario?.ClienteNome;

        // Status: aceita um ou vários (separados por vírgula) para multisseleção
        var statusSelecionados = string.IsNullOrWhiteSpace(status)
            ? new List<string>()
            : status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var statusParaServico = statusSelecionados.Count == 0 ? null
            : statusSelecionados.Count == 1 ? statusSelecionados[0]
            : null;

        var todos = await _origen.GetIsotanquesAsync(cliente, statusParaServico, busca);
        if (statusSelecionados.Count > 1)
            todos = todos.Where(c => statusSelecionados.Contains(c.Status)).ToList();

        var hoje = DateTime.Today;

        // Filtro "próximos N dias" (ex.: link do card Próximas liberações no Dashboard)
        if (proximos.HasValue && proximos.Value > 0)
        {
            var ate = hoje.AddDays(proximos.Value);
            todos = todos.Where(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= hoje && c.PrevisaoLiberacao.Value.Date <= ate).ToList();
        }

        var ordenado = ordenarPor switch
        {
            "Produto"            => ordemAsc ? todos.OrderBy(c => c.Produto)            : todos.OrderByDescending(c => c.Produto),
            "Status"             => ordemAsc ? todos.OrderBy(c => c.Status)             : todos.OrderByDescending(c => c.Status),
            "PrevisaoLiberacao"  => ordemAsc ? todos.OrderBy(c => c.PrevisaoLiberacao)  : todos.OrderByDescending(c => c.PrevisaoLiberacao),
            "Cliente"            => ordemAsc ? todos.OrderBy(c => c.Cliente)            : todos.OrderByDescending(c => c.Cliente),
            "DiasNoStatus"       => ordemAsc ? todos.OrderBy(c => c.DiasNoStatus ?? int.MaxValue) : todos.OrderByDescending(c => c.DiasNoStatus ?? -1),
            _                    => ordemAsc ? todos.OrderBy(c => c.Codigo)             : todos.OrderByDescending(c => c.Codigo),
        };

        var totalItens = todos.Count;
        var totalPaginas = totalItens == 0 ? 1 : (int)Math.Ceiling(totalItens / (double)itensPorPagina);
        pagina = Math.Min(pagina, totalPaginas);

        var lista = ordenado
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToList();

        // Dropdown de cliente só para Admin
        var clientes = ehAdmin ? await _origen.GetClientesAsync() : new List<string>();
        var statusList = await _origen.GetStatusListAsync();

        // Cards: contagens a partir da lista filtrada (todos) — mesma fonte para bater
        var porStatus = todos
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Status) ? "(sem etapa)" : c.Status.Trim())
            .ToDictionary(g => g.Key, g => g.Count());
        var estoqueTotal = porStatus.Values.Sum();
        var alertasDias = todos.Count(c => (c.DiasNoStatus ?? 0) >= 15);
        var aguardandoAcao = todos.Count(c => c.Status == "Ag. Envio Estimativa");
        var emSeteDias = hoje.AddDays(7);
        var proximasLiberacoes = todos.Count(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= hoje && c.PrevisaoLiberacao.Value.Date <= emSeteDias);

        ViewBag.EhAdmin = ehAdmin;
        ViewBag.Busca = busca;
        ViewBag.Cliente = cliente;
        ViewBag.Status = status;
        ViewBag.StatusSelecionados = statusSelecionados;
        ViewBag.OrdenarPor = ordenarPor;
        ViewBag.OrdemAsc = ordemAsc;
        ViewBag.EstoqueTotal = estoqueTotal;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalItens = totalItens;
        ViewBag.ItensPorPagina = itensPorPagina;
        ViewBag.Clientes = clientes;
        ViewBag.StatusList = statusList;
        ViewBag.PorStatus = porStatus;
        ViewBag.AlertasDias = alertasDias;
        ViewBag.AguardandoAcao = aguardandoAcao;
        ViewBag.ProximasLiberacoes = proximasLiberacoes;
        ViewBag.ProximosDias = proximos;
        ViewBag.UltimaAtualizacao = DateTime.Now;
        return View(lista);
    }

    /// <summary>Exporta a lista filtrada do inventário para Excel.</summary>
    [HttpGet]
    public async Task<IActionResult> ExportarExcel(
        string? busca,
        string? cliente,
        string? status,
        int? proximos,
        string? ordenarPor = "Codigo",
        bool ordemAsc = true,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        if (!ehAdmin)
            cliente = usuario?.ClienteNome;

        var statusSelecionadosExcel = string.IsNullOrWhiteSpace(status)
            ? new List<string>()
            : status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var statusParaServicoExcel = statusSelecionadosExcel.Count == 0 ? null : statusSelecionadosExcel.Count == 1 ? statusSelecionadosExcel[0] : null;

        var todosExcel = await _origen.GetIsotanquesAsync(cliente, statusParaServicoExcel, busca);
        if (statusSelecionadosExcel.Count > 1)
            todosExcel = todosExcel.Where(c => statusSelecionadosExcel.Contains(c.Status)).ToList();

        var hojeExcel = DateTime.Today;
        if (proximos.HasValue && proximos.Value > 0)
        {
            var ate = hojeExcel.AddDays(proximos.Value);
            todosExcel = todosExcel.Where(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= hojeExcel && c.PrevisaoLiberacao.Value.Date <= ate).ToList();
        }

        var ordenadoExcel = ordenarPor switch
        {
            "Produto"            => ordemAsc ? todosExcel.OrderBy(c => c.Produto)            : todosExcel.OrderByDescending(c => c.Produto),
            "Status"             => ordemAsc ? todosExcel.OrderBy(c => c.Status)             : todosExcel.OrderByDescending(c => c.Status),
            "PrevisaoLiberacao"  => ordemAsc ? todosExcel.OrderBy(c => c.PrevisaoLiberacao)  : todosExcel.OrderByDescending(c => c.PrevisaoLiberacao),
            "Cliente"            => ordemAsc ? todosExcel.OrderBy(c => c.Cliente)            : todosExcel.OrderByDescending(c => c.Cliente),
            "DiasNoStatus"       => ordemAsc ? todosExcel.OrderBy(c => c.DiasNoStatus ?? int.MaxValue) : todosExcel.OrderByDescending(c => c.DiasNoStatus ?? -1),
            _                    => ordemAsc ? todosExcel.OrderBy(c => c.Codigo)             : todosExcel.OrderByDescending(c => c.Codigo),
        };
        var listaExcel = ordenadoExcel.ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Inventário");
        ws.Cell(1, 1).Value = "Código";
        ws.Cell(1, 2).Value = "Produto";
        ws.Cell(1, 3).Value = "Cliente";
        ws.Cell(1, 4).Value = "Etapa";
        ws.Cell(1, 5).Value = "Prev. Liberação";
        ws.Cell(1, 6).Value = "Dias no status";
        var headerRow = ws.Range(1, 1, 1, 6);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in listaExcel)
        {
            ws.Cell(row, 1).Value = item.Codigo;
            ws.Cell(row, 2).Value = item.Produto;
            ws.Cell(row, 3).Value = item.Cliente;
            ws.Cell(row, 4).Value = item.Status;
            ws.Cell(row, 5).Value = item.PrevisaoLiberacao?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(row, 6).Value = item.DiasNoStatus.HasValue ? item.DiasNoStatus.Value.ToString() : "";
            row++;
        }
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        var fileName = $"Inventario_Isotanques_{hojeExcel:yyyyMMdd_HHmm}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>Gera PDF do inventário com a lista filtrada.</summary>
    [HttpGet]
    public async Task<IActionResult> ExportarPdf(
        string? busca,
        string? cliente,
        string? status,
        int? proximos,
        string? ordenarPor = "Codigo",
        bool ordemAsc = true,
        CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        if (!ehAdmin)
            cliente = usuario?.ClienteNome;

        var statusSelecionadosPdf = string.IsNullOrWhiteSpace(status)
            ? new List<string>()
            : status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var statusParaServicoPdf = statusSelecionadosPdf.Count == 0 ? null : statusSelecionadosPdf.Count == 1 ? statusSelecionadosPdf[0] : null;

        var todosPdf = await _origen.GetIsotanquesAsync(cliente, statusParaServicoPdf, busca);
        if (statusSelecionadosPdf.Count > 1)
            todosPdf = todosPdf.Where(c => statusSelecionadosPdf.Contains(c.Status)).ToList();

        var hojePdf = DateTime.Today;
        if (proximos.HasValue && proximos.Value > 0)
        {
            var ate = hojePdf.AddDays(proximos.Value);
            todosPdf = todosPdf.Where(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= hojePdf && c.PrevisaoLiberacao.Value.Date <= ate).ToList();
        }

        var ordenadoPdf = ordenarPor switch
        {
            "Produto"            => ordemAsc ? todosPdf.OrderBy(c => c.Produto)            : todosPdf.OrderByDescending(c => c.Produto),
            "Status"             => ordemAsc ? todosPdf.OrderBy(c => c.Status)             : todosPdf.OrderByDescending(c => c.Status),
            "PrevisaoLiberacao"  => ordemAsc ? todosPdf.OrderBy(c => c.PrevisaoLiberacao)  : todosPdf.OrderByDescending(c => c.PrevisaoLiberacao),
            "Cliente"            => ordemAsc ? todosPdf.OrderBy(c => c.Cliente)            : todosPdf.OrderByDescending(c => c.Cliente),
            "DiasNoStatus"       => ordemAsc ? todosPdf.OrderBy(c => c.DiasNoStatus ?? int.MaxValue) : todosPdf.OrderByDescending(c => c.DiasNoStatus ?? -1),
            _                    => ordemAsc ? todosPdf.OrderBy(c => c.Codigo)             : todosPdf.OrderByDescending(c => c.Codigo),
        };
        var listaPdf = ordenadoPdf.ToList();

        var stream = new MemoryStream();
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Text("Inventário de Isotanques — Portal do Cliente Depotce").SemiBold().FontSize(12);
                    row.RelativeItem().AlignRight().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm} — {listaPdf.Count} registro(s)").FontSize(9);
                });

                page.Content().PaddingTop(0.5f, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(90);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(75);
                        columns.ConstantColumn(45);
                    });
                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Padding(4).Text("Código").SemiBold();
                        header.Cell().BorderBottom(1).Padding(4).Text("Produto").SemiBold();
                        header.Cell().BorderBottom(1).Padding(4).Text("Cliente").SemiBold();
                        header.Cell().BorderBottom(1).Padding(4).Text("Etapa").SemiBold();
                        header.Cell().BorderBottom(1).Padding(4).Text("Prev. Liberação").SemiBold();
                        header.Cell().BorderBottom(1).Padding(4).Text("Dias").SemiBold();
                    });
                    foreach (var item in listaPdf)
                    {
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(item.Codigo);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(item.Produto ?? "");
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(item.Cliente ?? "");
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(item.Status ?? "");
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(item.PrevisaoLiberacao?.ToString("dd/MM/yyyy") ?? "–");
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(item.DiasNoStatus.HasValue ? item.DiasNoStatus.ToString()! : "–");
                    }
                });
            });
        }).GeneratePdf(stream);
        stream.Position = 0;
        var fileName = $"Inventario_Isotanques_{hojePdf:yyyyMMdd_HHmm}.pdf";
        return File(stream.ToArray(), "application/pdf", fileName);
    }

    public async Task<IActionResult> Detalhe(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return RedirectToAction(nameof(Index));

        var item = await _origen.GetIsotanquePorCodigoAsync(codigo.Trim());

        if (item == null)
        {
            TempData["Mensagem"] = $"Isotank '{codigo}' não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        // Cliente comum não pode acessar Isotank de outro cliente
        if (!User.IsInRole("Admin"))
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (item.Cliente != usuario?.ClienteNome)
            {
                TempData["Mensagem"] = "Acesso negado a este Isotank.";
                return RedirectToAction(nameof(Index));
            }
        }

        var codigoNorm = codigo.Trim();
        List<ObservacaoIsotank> observacoes;
        try
        {
            observacoes = await _db.ObservacoesIsotank
                .AsNoTracking()
                .Where(o => o.CodigoIsotank == codigoNorm)
                .OrderByDescending(o => o.DataHora)
                .ToListAsync(cancellationToken: default);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            observacoes = new List<ObservacaoIsotank>();
        }
        ViewBag.Observacoes = observacoes;

        return View(item);
    }

    /// <summary>Página de laudos e documentos (EIR + Certificado de Lavagem) por isotank. Dados vêm do MBM quando disponíveis.</summary>
    public async Task<IActionResult> Documentos(string? busca)
    {
        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        var cliente = ehAdmin ? null : usuario?.ClienteNome;

        var lista = await _origen.GetIsotanquesAsync(cliente, null, busca);
        lista = lista.OrderBy(c => c.Codigo).ToList();

        ViewBag.Busca = busca ?? "";
        return View(lista);
    }

    /// <summary>Gera PDF com os dados do isotank (detalhe da unidade).</summary>
    [HttpGet]
    public async Task<IActionResult> ExportarDetalhePdf(string? codigo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return RedirectToAction(nameof(Index));

        var item = await _origen.GetIsotanquePorCodigoAsync(codigo.Trim(), cancellationToken);
        if (item == null)
        {
            TempData["Mensagem"] = $"Isotank '{codigo}' não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        if (!User.IsInRole("Admin"))
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (item.Cliente != usuario?.ClienteNome)
            {
                TempData["Mensagem"] = "Acesso negado a este Isotank.";
                return RedirectToAction(nameof(Index));
            }
        }

        QuestPDF.Settings.License = LicenseType.Community;
        var stream = new MemoryStream();
        var geradoEm = DateTime.Now;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Text($"Isotank {item.Codigo} — Portal do Cliente Depotce").SemiBold().FontSize(12);
                    row.RelativeItem().AlignRight().Text(geradoEm.ToString("dd/MM/yyyy HH:mm")).FontSize(9);
                });

                page.Content().PaddingTop(0.8f, Unit.Centimetre).Column(column =>
                {
                    column.Item().Text("Identificação").SemiBold().FontSize(11);
                    column.Item().PaddingBottom(8).Row(r =>
                    {
                        r.RelativeItem().Text($"Código: {item.Codigo}");
                        if (!string.IsNullOrWhiteSpace(item.Tipo)) r.RelativeItem().Text($"Tipo: {item.Tipo}");
                    });
                    column.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Produto: {item.Produto ?? "–"}");
                        r.RelativeItem().Text($"Cliente: {item.Cliente ?? "–"}");
                    });
                    if (!string.IsNullOrWhiteSpace(item.ProprietarioArmador))
                        column.Item().PaddingBottom(12).Text($"Proprietário/Armador: {item.ProprietarioArmador}");

                    column.Item().PaddingTop(4).Text("Situação").SemiBold().FontSize(11);
                    column.Item().PaddingBottom(8).Row(r =>
                    {
                        r.RelativeItem().Text($"Status: {item.Status ?? "–"}");
                        r.RelativeItem().Text($"Dias no status: {(item.DiasNoStatus.HasValue ? item.DiasNoStatus.ToString() : "–")}");
                    });
                    column.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Previsão de liberação: {(item.PrevisaoLiberacao?.ToString("dd/MM/yyyy") ?? "–")}");
                    });

                    column.Item().PaddingTop(4).Text("Movimentação").SemiBold().FontSize(11);
                    column.Item().PaddingBottom(4).Text($"Descarregado no pátio: {(item.DataHoraDescarregadoPatio?.ToString("dd/MM/yyyy HH:mm") ?? "–")}");
                    column.Item().PaddingBottom(4).Text($"Carregado no veículo: {(item.DataHoraCarregadoVeiculo?.ToString("dd/MM/yyyy HH:mm") ?? "–")} " + (string.IsNullOrWhiteSpace(item.PlacaVeiculo) ? "" : $"· {item.PlacaVeiculo}"));
                    column.Item().PaddingBottom(12).Text($"Previsão chegada terminal: {(item.PrevisaoChegadaTerminal?.ToString("dd/MM/yyyy HH:mm") ?? "–")}");

                    if (item.ReparoAcumuladoValor.HasValue && item.ReparoAcumuladoValor.Value > 0)
                        column.Item().PaddingBottom(4).Text($"Reparo acumulado: R$ {item.ReparoAcumuladoValor.Value:N2}");
                    if (item.TestePeriodicoVencimento.HasValue)
                        column.Item().Text($"Teste periódico (venc.): {item.TestePeriodicoVencimento.Value:dd/MM/yyyy}");
                });
            });
        }).GeneratePdf(stream);

        stream.Position = 0;
        var fileName = $"Isotank_{item.Codigo}_{geradoEm:yyyyMMdd_HHmm}.pdf";
        return File(stream.ToArray(), "application/pdf", fileName);
    }

    /// <summary>Exporta os dados do isotank para CSV (abre no Excel).</summary>
    public async Task<IActionResult> ExportarDetalheExcel(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return RedirectToAction(nameof(Index));

        var item = await _origen.GetIsotanquePorCodigoAsync(codigo.Trim());
        if (item == null)
        {
            TempData["Mensagem"] = $"Isotank '{codigo}' não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        if (!User.IsInRole("Admin"))
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (item.Cliente != usuario?.ClienteNome)
            {
                TempData["Mensagem"] = "Acesso negado a este Isotank.";
                return RedirectToAction(nameof(Index));
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Campo;Valor");
        sb.AppendLine($"Código;{EscapeCsv(item.Codigo)}");
        sb.AppendLine($"Produto;{EscapeCsv(item.Produto)}");
        sb.AppendLine($"Cliente;{EscapeCsv(item.Cliente)}");
        sb.AppendLine($"Status;{EscapeCsv(item.Status)}");
        sb.AppendLine($"Dias no status;{item.DiasNoStatus?.ToString() ?? "–"}");
        sb.AppendLine($"Previsão liberação;{(item.PrevisaoLiberacao?.ToString("dd/MM/yyyy") ?? "–")}");
        sb.AppendLine($"Descarregado no pátio;{(item.DataHoraDescarregadoPatio?.ToString("dd/MM/yyyy HH:mm") ?? "–")}");
        sb.AppendLine($"Carregado no veículo;{(item.DataHoraCarregadoVeiculo?.ToString("dd/MM/yyyy HH:mm") ?? "–")}");
        sb.AppendLine($"Placa veículo;{EscapeCsv(item.PlacaVeiculo ?? "–")}");
        sb.AppendLine($"Previsão chegada terminal;{(item.PrevisaoChegadaTerminal?.ToString("dd/MM/yyyy HH:mm") ?? "–")}");
        var csv = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var bom = System.Text.Encoding.UTF8.GetPreamble();
        var withBom = new byte[bom.Length + csv.Length];
        Buffer.BlockCopy(bom, 0, withBom, 0, bom.Length);
        Buffer.BlockCopy(csv, 0, withBom, bom.Length, csv.Length);
        var fileName = $"Isotank_{item.Codigo}_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(withBom, "text/csv; charset=utf-8", fileName);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    /// <summary>Dashboard com totais por etapa, alertas e previsões. Admin vê tabela por cliente.</summary>
    public async Task<IActionResult> Dashboard()
    {
        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        var cliente = ehAdmin ? null : usuario?.ClienteNome;

        var todos = await _origen.GetIsotanquesAsync(cliente, null, null);
        var hoje = DateTime.Today;
        var emSeteDias = hoje.AddDays(7);

        var porStatus = todos.GroupBy(c => c.Status).ToDictionary(g => g.Key, g => g.Count());
        int GetStatus(string s) => porStatus.TryGetValue(s, out var n) ? n : 0;

        ViewBag.EhAdmin = ehAdmin;
        ViewBag.NomeCompleto = usuario?.NomeCompleto ?? User.Identity?.Name ?? "Usuário";
        ViewBag.EstoqueTotal = todos.Count;
        ViewBag.AgLimpeza = GetStatus("Ag. Limpeza");
        ViewBag.AgReparo = GetStatus("Ag. Reparo");
        ViewBag.AgInspecao = GetStatus("Ag. Inspeção");
        ViewBag.AgEnvioEstimativa = GetStatus("Ag. Envio Estimativa");
        ViewBag.AgOffHire = GetStatus("Ag. Off Hire");
        ViewBag.AlertasDias = todos.Count(c => (c.DiasNoStatus ?? 0) >= 10);
        ViewBag.ProximasLiberacoes = todos.Count(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= hoje && c.PrevisaoLiberacao.Value.Date <= emSeteDias);
        ViewBag.Cliente = cliente;

        var proximasLista = todos
            .Where(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= hoje && c.PrevisaoLiberacao.Value.Date <= emSeteDias)
            .OrderBy(c => c.PrevisaoLiberacao)
            .Take(5)
            .ToList();
        ViewBag.ProximasLiberacoesLista = proximasLista;

        var porProduto = todos
            .GroupBy(c => c.Produto ?? "(sem produto)")
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => new { Produto = g.Key, Total = g.Count() })
            .ToList();
        ViewBag.ProdutosLabels = porProduto.Select(p => p.Produto).ToList();
        ViewBag.ProdutosData = porProduto.Select(p => p.Total).ToList();

        if (ehAdmin)
        {
            var porCliente = todos
                .GroupBy(c => c.Cliente)
                .Select(g => new { Cliente = g.Key, Total = g.Count(), Lista = g.ToList() })
                .OrderBy(x => x.Cliente)
                .ToList();
            ViewBag.PorCliente = porCliente;
        }

        // Disponibilidade de estoque (saldo) — reservados = com NumeroBooking; demais por status
        ViewBag.DisponibilidadeReservados = todos.Count(c => !string.IsNullOrWhiteSpace(c.NumeroBooking));
        ViewBag.DisponibilidadeEmManutencao = GetStatus("Ag. Limpeza") + GetStatus("Ag. Reparo") + GetStatus("Ag. Inspeção");
        ViewBag.DisponibilidadeDisponivel = GetStatus("Ag. Envio Estimativa") + GetStatus("Ag. Off Hire");

        ViewBag.UltimaAtualizacao = DateTime.Now;
        return View();
    }

    /// <summary>Isotanks críticos: parados (DiasNoStatus >= 10) e previsões vencidas/próximas (até 3 dias).</summary>
    public async Task<IActionResult> Alertas()
    {
        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        var cliente = ehAdmin ? null : usuario?.ClienteNome;

        var todos = await _origen.GetIsotanquesAsync(cliente, null, null);
        var hoje = DateTime.Today;
        var limitePrevisao = hoje.AddDays(3);

        var parados = todos
            .Where(c => (c.DiasNoStatus ?? 0) >= 10)
            .OrderByDescending(c => c.DiasNoStatus ?? 0)
            .ThenBy(c => c.Codigo)
            .ToList();
        var previsoes = todos
            .Where(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date <= limitePrevisao)
            .OrderBy(c => c.PrevisaoLiberacao)
            .ThenBy(c => c.Codigo)
            .ToList();

        var codigosCriticos = parados.Select(c => c.Codigo).Union(previsoes.Select(c => c.Codigo)).ToHashSet();
        ViewBag.EhAdmin = ehAdmin;
        ViewBag.Parados = parados;
        ViewBag.Previsoes = previsoes;
        ViewBag.TotalCriticos = codigosCriticos.Count;
        ViewBag.UltimaAtualizacao = DateTime.Now;
        return View();
    }

    /// <summary>Tela para o cliente entrar em contato com a Depotce (WhatsApp / e-mail). Acessível sem login (ex.: "Esqueci minha senha").</summary>
    [AllowAnonymous]
    public async Task<IActionResult> Contato(string? codigo)
    {
        var config = await _configContato.GetAsync();
        ViewBag.WhatsAppNumero = config?.WhatsAppNumero?.Trim() ?? _configuration["Contato:WhatsAppNumero"]?.Trim() ?? "";
        ViewBag.Email = config?.EmailDestino?.Trim() ?? _configuration["Contato:Email"]?.Trim() ?? "";
        ViewBag.NomeEquipe = config?.NomeEquipe?.Trim() ?? _configuration["Contato:NomeEquipe"]?.Trim() ?? "Depotce";
        ViewBag.CodigoPre = codigo ?? "";

        Container? isotanque = null;
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            isotanque = await _origen.GetIsotanquePorCodigoAsync(codigo.Trim());
            if (isotanque != null && !User.IsInRole("Admin"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (isotanque.Cliente != usuario?.ClienteNome)
                    isotanque = null;
            }
        }
        ViewBag.Isotanque = isotanque;

        var smtpHost = config?.SmtpHost?.Trim() ?? _configuration["Contato:SmtpHost"]?.Trim();
        var smtpUser = config?.SmtpUser?.Trim() ?? _configuration["Contato:SmtpUser"]?.Trim();
        ViewBag.EmailEnviadoPeloSite = !string.IsNullOrWhiteSpace(smtpHost) && !string.IsNullOrWhiteSpace(smtpUser);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> EnviarEmail(string? Codigo, string? Assunto, string? Mensagem)
    {
        var config = await _configContato.GetAsync();
        var destino = config?.EmailDestino?.Trim() ?? _configuration["Contato:Email"]?.Trim();
        if (string.IsNullOrWhiteSpace(destino))
        {
            TempData["ContatoErro"] = "E-mail de destino não configurado. Um administrador pode definir em Configurações de contato.";
            return RedirectToAction(nameof(Contato), new { codigo = Codigo });
        }

        var smtpHost = config?.SmtpHost?.Trim() ?? _configuration["Contato:SmtpHost"]?.Trim();
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            TempData["ContatoErro"] = "SMTP não configurado. Um administrador pode definir a conta de envio em Configurações de contato.";
            return RedirectToAction(nameof(Contato), new { codigo = Codigo });
        }

        var usuario = await _userManager.GetUserAsync(User);
        var nomeEquipe = config?.NomeEquipe?.Trim() ?? _configuration["Contato:NomeEquipe"]?.Trim() ?? "Depotce";
        var codigo = (Codigo ?? "").Trim().ToUpper();
        var assunto = (Assunto ?? "").Trim();
        if (string.IsNullOrWhiteSpace(assunto)) assunto = "Solicitação";

        var corpo = $"Olá, {nomeEquipe}!";
        if (!string.IsNullOrEmpty(codigo)) corpo += $"\r\n\r\nIsotank: {codigo}";
        corpo += $"\r\nAssunto: {assunto}";
        if (!string.IsNullOrWhiteSpace(Mensagem)) corpo += $"\r\n\r\n{Mensagem}";
        if (string.IsNullOrEmpty(codigo) && string.IsNullOrWhiteSpace(Mensagem)) corpo += "\r\n\r\nGostaria de obter informações.";
        corpo += $"\r\n\r\n---\r\nEnviado pelo Portal do Cliente por {usuario?.NomeCompleto ?? User.Identity?.Name ?? "Cliente"} ({usuario?.Email ?? "—"})";

        var subject = string.IsNullOrEmpty(codigo)
            ? $"[Portal Depotce] {assunto}"
            : $"[Portal Depotce] {assunto} – {codigo}";

        var smtpUser = config?.SmtpUser?.Trim() ?? _configuration["Contato:SmtpUser"]?.Trim();
        var smtpPassword = !string.IsNullOrEmpty(config?.SmtpUser)
            ? await _configContato.GetSmtpPasswordPlainAsync()
            : _configuration["Contato:SmtpPassword"];

        try
        {
            var fromEmail = config?.FromEmail?.Trim() ?? _configuration["Contato:FromEmail"]?.Trim() ?? destino;
            var fromName = config?.FromName?.Trim() ?? _configuration["Contato:FromName"]?.Trim() ?? "Portal do Cliente Depotce";
            if (string.IsNullOrWhiteSpace(fromEmail) && !string.IsNullOrEmpty(smtpUser))
                fromEmail = smtpUser;

            using var client = new SmtpClient(smtpHost)
            {
                Port = config?.SmtpPort ?? (int.TryParse(_configuration["Contato:SmtpPort"], out var p) ? p : 587),
                EnableSsl = config?.SmtpEnableSsl ?? string.Equals(_configuration["Contato:SmtpEnableSsl"], "true", StringComparison.OrdinalIgnoreCase),
                Credentials = !string.IsNullOrEmpty(smtpUser) ? new NetworkCredential(smtpUser, smtpPassword ?? "") : null
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = corpo,
                IsBodyHtml = false
            };
            mail.To.Add(destino);
            if (!string.IsNullOrWhiteSpace(usuario?.Email))
                mail.ReplyToList.Add(new MailAddress(usuario.Email, usuario.NomeCompleto ?? usuario.Email));

            await client.SendMailAsync(mail);
            TempData["ContatoSucesso"] = "E-mail enviado com sucesso. Nossa equipe responderá em breve.";
        }
        catch (Exception)
        {
            TempData["ContatoErro"] = "Não foi possível enviar o e-mail. Tente novamente ou use o WhatsApp.";
        }

        return RedirectToAction(nameof(Contato), new { codigo = Codigo });
    }

    /// <summary>Relatórios BI — visão analítica do estoque (totais por status/cliente, tempos médios, movimentações, gráficos).</summary>
    public async Task<IActionResult> Relatorios(string? cliente, string? status)
    {
        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        var clienteFiltro = ehAdmin ? (string.IsNullOrWhiteSpace(cliente) ? null : cliente) : usuario?.ClienteNome;

        var todos = await _origen.GetIsotanquesAsync(clienteFiltro, null, null);
        if (!string.IsNullOrWhiteSpace(status))
            todos = todos.Where(c => c.Status == status).ToList();

        ViewBag.FiltroCliente = cliente;
        ViewBag.FiltroStatus = status;

        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        // Total por status
        var totalPorStatus = todos
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key, g => g.Count());
        ViewBag.TotalPorStatus = totalPorStatus;

        // Total por cliente
        var totalPorCliente = todos
            .GroupBy(c => c.Cliente)
            .ToDictionary(g => g.Key, g => g.Count());
        ViewBag.TotalPorCliente = totalPorCliente;

        // Tempo médio por status (média de DiasNoStatus)
        var tempoMedioPorStatus = todos
            .Where(c => c.DiasNoStatus.HasValue)
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key, g => g.Average(c => c.DiasNoStatus!.Value));
        ViewBag.TempoMedioPorStatus = tempoMedioPorStatus;

        // Maior tempo parado
        var comDias = todos.Where(c => c.DiasNoStatus.HasValue).ToList();
        var maiorTempo = comDias.OrderByDescending(c => c.DiasNoStatus).FirstOrDefault();
        ViewBag.MaiorTempoCodigo = maiorTempo?.Codigo ?? "";
        ViewBag.MaiorTempoCliente = maiorTempo?.Cliente ?? "";
        ViewBag.MaiorTempoDias = maiorTempo?.DiasNoStatus ?? 0;

        // Tempo médio geral
        var tempoMedioGeral = comDias.Any() ? comDias.Average(c => c.DiasNoStatus!.Value) : 0.0;
        ViewBag.TempoMedioGeral = Math.Round(tempoMedioGeral, 1);

        // Movimentações mensais (entradas = DataHoraDescarregadoPatio) — últimos 6 meses
        var seisMesesAtras = hoje.AddMonths(-5);
        var inicioSeis = new DateTime(seisMesesAtras.Year, seisMesesAtras.Month, 1);
        var entradasPorMes = todos
            .Where(c => c.DataHoraDescarregadoPatio.HasValue && c.DataHoraDescarregadoPatio.Value >= inicioSeis)
            .GroupBy(c => new { c.DataHoraDescarregadoPatio!.Value.Year, c.DataHoraDescarregadoPatio.Value.Month })
            .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => g.Count());
        var saidasPorMes = todos
            .Where(c => c.DataHoraCarregadoVeiculo.HasValue && c.DataHoraCarregadoVeiculo.Value >= inicioSeis)
            .GroupBy(c => new { c.DataHoraCarregadoVeiculo!.Value.Year, c.DataHoraCarregadoVeiculo.Value.Month })
            .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => g.Count());
        var mesesLabels = new List<string>();
        var entradasValores = new List<int>();
        var saidasValores = new List<int>();
        var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        for (var d = inicioSeis; d <= hoje; d = d.AddMonths(1))
        {
            var key = $"{d.Year}-{d.Month:D2}";
            var mesAno = d.ToString("MMM/yy", culture).Replace(".", "");
            mesesLabels.Add(char.ToUpperInvariant(mesAno[0]) + mesAno[1..]);
            entradasValores.Add(entradasPorMes.TryGetValue(key, out var e) ? e : 0);
            saidasValores.Add(saidasPorMes.TryGetValue(key, out var s) ? s : 0);
        }
        ViewBag.MovimentacoesMensaisLabels = mesesLabels;
        ViewBag.MovimentacoesMensaisEntradas = entradasValores;
        ViewBag.MovimentacoesMensaisSaidas = saidasValores;

        // Taxa de reparo (% em Ag. Reparo)
        var total = todos.Count;
        var emReparo = totalPorStatus.TryGetValue("Ag. Reparo", out var rep) ? rep : 0;
        ViewBag.TaxaReparo = total > 0 ? Math.Round(100.0 * emReparo / total, 1) : 0.0;

        // Liberações no mês (PrevisaoLiberacao no mês atual)
        var liberacoesMes = todos.Count(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= inicioMes && c.PrevisaoLiberacao.Value.Date <= fimMes);
        ViewBag.LiberacoesMes = liberacoesMes;

        // Dias médio por cliente e status (para gráfico agrupado)
        var statusOrdem = new[] { "Ag. Limpeza", "Ag. Reparo", "Ag. Inspeção", "Ag. Envio Estimativa", "Ag. Off Hire" };
        var clientesOrdenados = totalPorCliente.Keys.OrderBy(x => x).ToList();
        var diasMedioPorClienteEStatus = new Dictionary<string, Dictionary<string, double>>();
        foreach (var cli in clientesOrdenados)
        {
            var porStatus = todos
                .Where(c => c.Cliente == cli && c.DiasNoStatus.HasValue)
                .GroupBy(c => c.Status)
                .ToDictionary(g => g.Key, g => g.Average(c => c.DiasNoStatus!.Value));
            var dict = new Dictionary<string, double>();
            foreach (var st in statusOrdem)
                dict[st] = porStatus.TryGetValue(st, out var v) ? Math.Round(v, 1) : 0;
            diasMedioPorClienteEStatus[cli] = dict;
        }
        ViewBag.StatusesOrdenados = statusOrdem;
        ViewBag.ClientesOrdenados = clientesOrdenados;
        ViewBag.DiasMedioPorClienteEStatus = diasMedioPorClienteEStatus;

        ViewBag.EhAdmin = ehAdmin;
        ViewBag.ClienteNome = clienteFiltro; // nome do cliente quando perfil Cliente (null para Admin)
        ViewBag.UltimaAtualizacao = DateTime.Now;
        return View();
    }

    /// <summary>Relatórios — Bookings: bookings ativos, isotanks reservados/disponíveis, gráficos e tabela.</summary>
    public async Task<IActionResult> RelatoriosBookings(int? periodoDias, string? booking, string? produto, string? status)
    {
        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        var clienteFiltro = ehAdmin ? null : usuario?.ClienteNome;

        var todos = await _origen.GetIsotanquesAsync(clienteFiltro, null, null);

        // Listas para os dropdowns (antes de filtrar)
        var todosCompletos = new List<Container>(todos);
        ViewBag.BookingsList = todosCompletos.Where(c => !string.IsNullOrWhiteSpace(c.NumeroBooking)).Select(c => c.NumeroBooking!).Distinct().OrderBy(x => x).ToList();
        ViewBag.StatusList = todosCompletos.Select(c => c.Status).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(x => x).ToList();
        ViewBag.ProdutosList = todosCompletos.Select(c => c.Produto ?? "(sem produto)").Distinct().OrderBy(x => x).ToList();

        // Aplicar filtros
        if (periodoDias.HasValue && periodoDias.Value > 0)
        {
            var limite = DateTime.Today.AddDays(-periodoDias.Value);
            todos = todos.Where(c =>
            {
                var d = c.DataSaida ?? c.PrevisaoLiberacao;
                return d.HasValue && d.Value.Date >= limite;
            }).ToList();
        }
        if (!string.IsNullOrWhiteSpace(booking))
            todos = todos.Where(c => c.NumeroBooking == booking).ToList();
        if (!string.IsNullOrWhiteSpace(produto))
            todos = todos.Where(c => (c.Produto ?? "") == produto).ToList();
        if (!string.IsNullOrWhiteSpace(status))
            todos = todos.Where(c => c.Status == status).ToList();

        var comBooking = todos.Where(c => !string.IsNullOrWhiteSpace(c.NumeroBooking)).ToList();
        var bookingsAtivos = comBooking.Select(c => c.NumeroBooking).Distinct().Count();
        var disponiveis = todos.Count(c => string.IsNullOrWhiteSpace(c.NumeroBooking)
            && c.Status != "Ag. Reparo"
            && c.Status != "Ag. Inspeção");
        var manutencao = todos.Count(c => c.Status == "Ag. Reparo" || c.Status == "Ag. Inspeção");

        ViewBag.BookingsAtivos = bookingsAtivos;
        ViewBag.ReservadosBooking = comBooking.Count;
        ViewBag.Disponiveis = disponiveis;
        ViewBag.Manutencao = manutencao;
        ViewBag.IsotanksComBooking = comBooking;
        ViewBag.TotalGeral = todos.Count;

        ViewBag.PorProdutoBooking = comBooking
            .GroupBy(c => c.Produto ?? "(sem produto)")
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        // Bookings abertos por mês (últimos 6 meses): por mês, quantos bookings distintos têm ao menos um isotank com PrevisaoLiberacao naquele mês
        var hoje = DateTime.Today;
        var seisMesesAtras = hoje.AddMonths(-5);
        var inicioSeis = new DateTime(seisMesesAtras.Year, seisMesesAtras.Month, 1);
        var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        var mesesLabels = new List<string>();
        var bookingsPorMes = new List<int>();
        for (var d = inicioSeis; d <= hoje; d = d.AddMonths(1))
        {
            var mesAno = d.ToString("MMM/yy", culture).Replace(".", "");
            mesesLabels.Add(char.ToUpperInvariant(mesAno[0]) + mesAno[1..]);
            var count = comBooking
                .Where(c => c.PrevisaoLiberacao.HasValue
                    && c.PrevisaoLiberacao.Value.Year == d.Year
                    && c.PrevisaoLiberacao.Value.Month == d.Month)
                .Select(c => c.NumeroBooking)
                .Distinct()
                .Count();
            bookingsPorMes.Add(count);
        }
        ViewBag.BookingsPorMesLabels = mesesLabels;
        ViewBag.BookingsPorMesValores = bookingsPorMes;

        ViewBag.EhAdmin = ehAdmin;
        ViewBag.ClienteNome = clienteFiltro;
        ViewBag.UltimaAtualizacao = DateTime.Now;
        ViewBag.PeriodoDias = periodoDias;
        ViewBag.FiltroBooking = booking;
        ViewBag.FiltroProduto = produto;
        ViewBag.FiltroStatus = status;
        return View();
    }

    /// <summary>Gera PDF do Relatório BI (indicadores + tempo por etapa + lista de isotanques).</summary>
    [HttpGet]
    public async Task<IActionResult> ExportarRelatoriosPdf(CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var usuario = await _userManager.GetUserAsync(User);
        var ehAdmin = User.IsInRole("Admin");
        var clienteFiltro = ehAdmin ? null : usuario?.ClienteNome;

        var todos = await _origen.GetIsotanquesAsync(clienteFiltro, null, null);
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var totalPorStatus = todos
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Status) ? "(sem etapa)" : c.Status.Trim())
            .ToDictionary(g => g.Key, g => g.Count());
        var comDias = todos.Where(c => c.DiasNoStatus.HasValue).ToList();
        var maiorTempo = comDias.OrderByDescending(c => c.DiasNoStatus).FirstOrDefault();
        var tempoMedioGeral = comDias.Any() ? comDias.Average(c => c.DiasNoStatus!.Value) : 0.0;
        var total = todos.Count;
        var emReparo = totalPorStatus.GetValueOrDefault("Ag. Reparo");
        var taxaReparo = total > 0 ? Math.Round(100.0 * emReparo / total, 1) : 0.0;
        var liberacoesMes = todos.Count(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date >= inicioMes && c.PrevisaoLiberacao.Value.Date <= fimMes);

        var tempoMedioPorStatus = todos
            .Where(c => c.DiasNoStatus.HasValue)
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key, g => g.Average(c => c.DiasNoStatus!.Value));
        var statusOrdemPdf = new[] { "Ag. Limpeza", "Ag. Reparo", "Ag. Inspeção", "Ag. Envio Estimativa", "Ag. Off Hire" };

        var listaOrdenada = todos.OrderBy(c => c.Codigo).ToList();
        var tituloEscopo = ehAdmin ? "Todos os clientes" : $"Seu estoque ({clienteFiltro})";

        var stream = new MemoryStream();
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Relatório BI — Portal do Cliente Depotce").SemiBold().FontSize(12);
                        row.RelativeItem().AlignRight().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                    });
                    column.Item().PaddingTop(4).Text($"Visão analítica — {tituloEscopo}").FontSize(10);
                });

                page.Content().PaddingTop(0.8f, Unit.Centimetre).Column(column =>
                {
                    // 4 indicadores em 2x2
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(90).Column(c =>
                        {
                            c.Item().Text("Tempo médio no pátio").FontSize(8).SemiBold();
                            c.Item().Text($"{Math.Round(tempoMedioGeral, 1)} dias").FontSize(11).Bold();
                        });
                        row.ConstantItem(90).Column(c =>
                        {
                            c.Item().Text("Maior tempo parado").FontSize(8).SemiBold();
                            c.Item().Text(maiorTempo != null ? $"{maiorTempo.DiasNoStatus} dias — {maiorTempo.Codigo}" : "–").FontSize(10).Bold();
                        });
                        row.ConstantItem(90).Column(c =>
                        {
                            c.Item().Text("Liberações no mês").FontSize(8).SemiBold();
                            c.Item().Text(liberacoesMes.ToString()).FontSize(11).Bold();
                        });
                        row.ConstantItem(90).Column(c =>
                        {
                            c.Item().Text("Taxa de reparo").FontSize(8).SemiBold();
                            c.Item().Text($"{taxaReparo}%").FontSize(11).Bold();
                        });
                    });

                    column.Item().PaddingTop(12).Text("Tempo médio por etapa").SemiBold().FontSize(10);
                    column.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.5f);
                            columns.ConstantColumn(50);
                        });
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).Padding(3).Text("Etapa").SemiBold();
                            header.Cell().BorderBottom(1).Padding(3).Text("Dias").SemiBold();
                        });
                        foreach (var st in statusOrdemPdf)
                        {
                            if (!tempoMedioPorStatus.TryGetValue(st, out var dias)) continue;
                            table.Cell().BorderBottom(0.5f).Padding(3).Text(st);
                            table.Cell().BorderBottom(0.5f).Padding(3).Text(Math.Round(dias, 1).ToString());
                        }
                    });

                    column.Item().PaddingTop(14).Text($"Inventário ({listaOrdenada.Count} registro(s))").SemiBold().FontSize(10);
                    column.Item().PaddingTop(4).Table(tab =>
                    {
                        tab.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(72);
                            cols.RelativeColumn(1.8f);
                            cols.RelativeColumn(1.2f);
                            cols.RelativeColumn(1.8f);
                            cols.ConstantColumn(58);
                            cols.ConstantColumn(32);
                        });
                        tab.Header(h =>
                        {
                            h.Cell().BorderBottom(1).Padding(3).Text("Código").SemiBold();
                            h.Cell().BorderBottom(1).Padding(3).Text("Produto").SemiBold();
                            h.Cell().BorderBottom(1).Padding(3).Text("Cliente").SemiBold();
                            h.Cell().BorderBottom(1).Padding(3).Text("Etapa").SemiBold();
                            h.Cell().BorderBottom(1).Padding(3).Text("Prev. Lib.").SemiBold();
                            h.Cell().BorderBottom(1).Padding(3).Text("Dias").SemiBold();
                        });
                        foreach (var item in listaOrdenada)
                        {
                            tab.Cell().BorderBottom(0.5f).Padding(3).Text(item.Codigo);
                            tab.Cell().BorderBottom(0.5f).Padding(3).Text(item.Produto ?? "");
                            tab.Cell().BorderBottom(0.5f).Padding(3).Text(item.Cliente ?? "");
                            tab.Cell().BorderBottom(0.5f).Padding(3).Text(item.Status ?? "");
                            tab.Cell().BorderBottom(0.5f).Padding(3).Text(item.PrevisaoLiberacao?.ToString("dd/MM/yyyy") ?? "–");
                            tab.Cell().BorderBottom(0.5f).Padding(3).Text(item.DiasNoStatus.HasValue ? item.DiasNoStatus.ToString()! : "–");
                        }
                    });
                });
            });
        }).GeneratePdf(stream);
        stream.Position = 0;
        var fileName = $"Relatorio_BI_Isotanques_{hoje:yyyyMMdd_HHmm}.pdf";
        return File(stream.ToArray(), "application/pdf", fileName);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}