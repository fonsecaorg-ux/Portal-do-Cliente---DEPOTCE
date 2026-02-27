using Microsoft.EntityFrameworkCore;
using SimuladorMBM.Data;
using SimuladorMBM.Models;

namespace SimuladorMBM;

/// <summary>
/// Rotas do painel estilo MBM real: dashboard de estoque + ficha do isotank.
/// </summary>
public static class PainelRoutes
{
    public static void MapPainelRoutes(this WebApplication app)
    {
        app.MapGet("/painel", async (MbmDbContext db, HttpContext ctx) =>
        {
            var clienteFiltro = ctx.Request.Query["cliente"].FirstOrDefault()?.Trim();
            var statusFiltro = ctx.Request.Query["status"].FirstOrDefault()?.Trim();
            var hoje = DateTime.Today;

            var totalIsotanques = await db.Isotanques.CountAsync();
            var totalClientes = await db.Clientes.CountAsync();
            var totalProdutos = await db.Produtos.CountAsync();
            var clientes = await db.Clientes.OrderBy(c => c.Nome).Select(c => c.Nome).ToListAsync();
            var statusList = await db.Isotanques.Select(i => i.Status).Distinct().OrderBy(s => s).ToListAsync();

            var query = db.Isotanques.Include(i => i.Cliente).Include(i => i.Produto).Where(i => i.Ativo);
            if (!string.IsNullOrEmpty(clienteFiltro)) query = query.Where(i => i.Cliente.Nome == clienteFiltro);
            if (!string.IsNullOrEmpty(statusFiltro)) query = query.Where(i => i.Status == statusFiltro);
            var lista = await query.OrderBy(i => i.Codigo).ToListAsync();

            var porStatus = await db.Isotanques.GroupBy(i => i.Status).Select(g => new { g.Key, Total = g.Count() }).OrderByDescending(x => x.Total).ToListAsync();
            var linhasStatus = string.Join("", porStatus.Select(s => $"<tr><td>{System.Net.WebUtility.HtmlEncode(s.Key)}</td><td>{s.Total}</td></tr>"));

            var linhasTab = string.Join("", lista.Select(i =>
            {
                var diasPatio = i.DataEntrada.HasValue ? (int)(hoje - i.DataEntrada.Value.Date).TotalDays : (int?)null;
                var diasStatus = i.DataInicioStatus.HasValue ? (int)(hoje - i.DataInicioStatus.Value.Date).TotalDays : (int?)null;
                var loc = new[] { i.Patio, i.Bloco, i.Fila, i.Pilha }.Where(s => !string.IsNullOrWhiteSpace(s));
                var locStr = string.Join(" / ", loc);
                return $@"<tr>
                    <td><a href=""/painel/isotank/{Uri.EscapeDataString(i.Codigo)}"">{System.Net.WebUtility.HtmlEncode(i.Codigo)}</a></td>
                    <td>{System.Net.WebUtility.HtmlEncode(i.Tipo ?? "–")}</td>
                    <td>{System.Net.WebUtility.HtmlEncode(i.Produto.Nome)}</td>
                    <td>{System.Net.WebUtility.HtmlEncode(i.Cliente.Nome)}</td>
                    <td>{System.Net.WebUtility.HtmlEncode(i.Status)}</td>
                    <td class=""num"">{diasPatio?.ToString() ?? "–"}</td>
                    <td class=""num"">{diasStatus?.ToString() ?? "–"}</td>
                    <td>{System.Net.WebUtility.HtmlEncode(locStr)}</td>
                    <td>{(i.SlaLimite?.ToString("dd/MM/yyyy HH:mm") ?? "–")}</td>
                    <td>{(i.PrevisaoLiberacao?.ToString("dd/MM/yyyy") ?? "–")}</td>
                    <td><a href=""/painel/isotank/{Uri.EscapeDataString(i.Codigo)}"" class=""btn-sm"">Detalhe</a></td>
                </tr>";
            }));
            var optsCliente = string.Join("", clientes.Select(c => $"<option value=\"{System.Net.WebUtility.HtmlEncode(c)}\"{(c == clienteFiltro ? " selected" : "")}>{System.Net.WebUtility.HtmlEncode(c)}</option>"));
            var optsStatus = string.Join("", statusList.Select(s => $"<option value=\"{System.Net.WebUtility.HtmlEncode(s)}\"{(s == statusFiltro ? " selected" : "")}>{System.Net.WebUtility.HtmlEncode(s)}</option>"));

            var html = $$"""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Dashboard de Estoque – MBM</title>
    <style>
        body { font-family: Segoe UI, sans-serif; max-width: 1200px; margin: 20px auto; padding: 20px; }
        h1 { color: #0d6efd; }
        h2 { margin-top: 24px; font-size: 1.1rem; color: #333; }
        .cards { display: flex; gap: 16px; flex-wrap: wrap; margin: 16px 0; }
        .card { background: #f0f4ff; border: 1px solid #0d6efd; border-radius: 8px; padding: 16px 24px; min-width: 120px; }
        .card strong { display: block; font-size: 1.8rem; color: #0d6efd; }
        .filtros { display: flex; gap: 12px; align-items: center; flex-wrap: wrap; margin: 16px 0; }
        .filtros select { padding: 6px 10px; border: 1px solid #ccc; border-radius: 4px; }
        .filtros .btn { padding: 6px 14px; background: #0d6efd; color: white; border: none; border-radius: 4px; cursor: pointer; text-decoration: none; font-size: 14px; }
        .filtros .btn:hover { background: #0b5ed7; }
        table { width: 100%; border-collapse: collapse; margin-top: 8px; font-size: 0.9rem; }
        th, td { border: 1px solid #ddd; padding: 6px 8px; text-align: left; }
        th { background: #0d6efd; color: white; }
        tr:nth-child(even) { background: #f9f9f9; }
        td.num { text-align: right; }
        a { color: #0d6efd; }
        .btn-sm { padding: 4px 10px; background: #0d6efd; color: white; text-decoration: none; border-radius: 4px; font-size: 12px; }
        .btn-sm:hover { background: #0b5ed7; color: white; }
    </style>
</head>
<body>
    <h1>Dashboard de Estoque – MBM</h1>
    <p>Visão operacional do sistema base (MBM). O <strong>Portal do Cliente</strong> consome a mesma API e exibe telas para o cliente.</p>
    <p><a href="/">← Página inicial</a></p>

    <h2>Resumo</h2>
    <div class="cards">
        <div class="card"><strong>{{totalIsotanques}}</strong> isotanques</div>
        <div class="card"><strong>{{totalClientes}}</strong> clientes</div>
        <div class="card"><strong>{{totalProdutos}}</strong> produtos</div>
    </div>

    <h2>Isotanques por status</h2>
    <table><thead><tr><th>Status</th><th>Quantidade</th></tr></thead><tbody>{{linhasStatus}}</tbody></table>

    <h2>Estoque (todos os isotanques)</h2>
    <form class="filtros" method="get" action="/painel">
        <label>Cliente: <select name="cliente"><option value="">Todos</option>{{optsCliente}}</select></label>
        <label>Status: <select name="status"><option value="">Todos</option>{{optsStatus}}</select></label>
        <button type="submit" class="btn">Filtrar</button>
        <a href="/painel" class="btn">Limpar</a>
    </form>
    <p><strong>{{lista.Count}}</strong> registro(s)</p>
    <div style="overflow-x: auto;">
    <table><thead><tr><th>Código</th><th>Tipo</th><th>Produto</th><th>Cliente</th><th>Status</th><th>Dias pátio</th><th>Dias status</th><th>Localização</th><th>SLA</th><th>Previsão lib.</th><th></th></tr></thead><tbody>{{linhasTab}}</tbody></table>
    </div>
</body>
</html>
""";
            return Results.Content(html, "text/html");
        });

        app.MapGet("/painel/isotank/{codigo}", async (MbmDbContext db, string codigo) =>
        {
            var hoje = DateTime.Today;
            var item = await db.Isotanques
                .Include(i => i.Cliente)
                .Include(i => i.Produto)
                .FirstOrDefaultAsync(i => i.Ativo && i.Codigo == codigo);
            if (item == null)
                return Results.NotFound("Isotanque não encontrado.");

            var diasPatio = item.DataEntrada.HasValue ? (int)(hoje - item.DataEntrada.Value.Date).TotalDays : (int?)null;
            var diasStatus = item.DataInicioStatus.HasValue ? (int)(hoje - item.DataInicioStatus.Value.Date).TotalDays : (int?)null;
            var loc = new[] { item.Patio, item.Bloco, item.Fila, item.Pilha }.Where(s => !string.IsNullOrWhiteSpace(s));
            var locStr = string.Join(" · ", loc);
            var fotos = string.IsNullOrWhiteSpace(item.UrlsFotos) ? (string.IsNullOrWhiteSpace(item.UrlFoto) ? Array.Empty<string>() : new[] { item.UrlFoto }) : item.UrlsFotos!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string Esc(string? s) => string.IsNullOrEmpty(s) ? "–" : System.Net.WebUtility.HtmlEncode(s);
            string Data(DateTime? d) => d?.ToString("dd/MM/yyyy HH:mm") ?? "–";
            string DataCurta(DateTime? d) => d?.ToString("dd/MM/yyyy") ?? "–";

            var fotosHtml = fotos.Length == 0 ? "<p>Nenhuma foto registrada.</p>" : string.Join("", fotos.Take(10).Select(u => $"<a href=\"{System.Net.WebUtility.HtmlEncode(u)}\" target=\"_blank\"><img src=\"{System.Net.WebUtility.HtmlEncode(u)}\" alt=\"Foto\" style=\"max-width:120px;max-height:90px;object-fit:cover;border:1px solid #ddd;border-radius:4px;margin:4px;\" /></a>"));
            var certHtml = string.IsNullOrEmpty(item.UrlCertificadoLavagem) ? "<p>Certificado de lavagem: –</p>" : $"<p>Certificado de lavagem: <a href=\"{System.Net.WebUtility.HtmlEncode(item.UrlCertificadoLavagem)}\" target=\"_blank\">Abrir / download</a></p>";
            var laudoHtml = string.IsNullOrEmpty(item.UrlLaudoVistoria) ? "<p>Laudo de vistoria (EIR): –</p>" : $"<p>Laudo de vistoria (EIR): <a href=\"{System.Net.WebUtility.HtmlEncode(item.UrlLaudoVistoria)}\" target=\"_blank\">Abrir / download</a></p>";
            var reparoStr = item.ReparoAcumuladoValor.HasValue ? item.ReparoAcumuladoValor.Value.ToString("N2") : "–";

            var codigoEsc = Esc(item.Codigo);
            var statusEsc = Esc(item.Status);
            var produtoEsc = Esc(item.Produto.Nome);
            var clienteEsc = Esc(item.Cliente.Nome);
            var tipoEsc = Esc(item.Tipo);
            var propEsc = Esc(item.ProprietarioArmador);
            var placaEsc = Esc(item.PlacaVeiculo);
            var diasStatusStr = diasStatus?.ToString() ?? "–";
            var diasPatioStr = diasPatio?.ToString() ?? "–";
            var dataEntradaStr = Data(item.DataEntrada);
            var dataInicioStr = Data(item.DataInicioStatus);
            var locStrEsc = string.IsNullOrEmpty(locStr) ? "–" : Esc(locStr);
            var slaStr = Data(item.SlaLimite);
            var prevLibStr = DataCurta(item.PrevisaoLiberacao);
            var prevTermStr = Data(item.PrevisaoChegadaTerminal);
            var descTermStr = Data(item.DataHoraDescarregadoTerminal);
            var descPatioStr = Data(item.DataHoraDescarregadoPatio);
            var carrVeicStr = Data(item.DataHoraCarregadoVeiculo);
            var testPerStr = DataCurta(item.TestePeriodicoVencimento);

            var html = $$"""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Isotank {{codigoEsc}} – MBM</title>
    <style>
        body { font-family: Segoe UI, sans-serif; max-width: 900px; margin: 20px auto; padding: 20px; }
        h1 { color: #0d6efd; font-size: 1.5rem; }
        h2 { margin-top: 20px; font-size: 1rem; color: #333; border-bottom: 1px solid #ddd; padding-bottom: 4px; }
        .grid { display: grid; grid-template-columns: 180px 1fr; gap: 6px 16px; }
        .grid dt { color: #666; }
        .grid dd { margin: 0; }
        .badge { display: inline-block; padding: 4px 10px; border-radius: 4px; background: #ffc107; color: #000; font-weight: 600; }
        a { color: #0d6efd; }
        .fotos { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 8px; }
    </style>
</head>
<body>
    <p><a href="/painel">← Voltar ao Dashboard</a></p>
    <h1>{{codigoEsc}} <span class="badge">{{statusEsc}}</span></h1>
    <p>{{produtoEsc}} – {{clienteEsc}}</p>

    <h2>Identificação</h2>
    <dl class="grid">
        <dt>Código</dt><dd>{{codigoEsc}}</dd>
        <dt>Tipo</dt><dd>{{tipoEsc}}</dd>
        <dt>Proprietário / Armador</dt><dd>{{propEsc}}</dd>
        <dt>Produto (última carga)</dt><dd>{{produtoEsc}}</dd>
        <dt>Cliente</dt><dd>{{clienteEsc}}</dd>
    </dl>

    <h2>Status operacional</h2>
    <dl class="grid">
        <dt>Status</dt><dd>{{statusEsc}}</dd>
        <dt>Dias no status</dt><dd>{{diasStatusStr}}</dd>
        <dt>Dias no pátio</dt><dd>{{diasPatioStr}}</dd>
        <dt>Entrada no pátio</dt><dd>{{dataEntradaStr}}</dd>
        <dt>Início do status atual</dt><dd>{{dataInicioStr}}</dd>
    </dl>

    <h2>Localização</h2>
    <dl class="grid">
        <dt>Pátio / Bloco / Fila / Pilha</dt><dd>{{locStrEsc}}</dd>
    </dl>

    <h2>SLA e previsões</h2>
    <dl class="grid">
        <dt>SLA limite</dt><dd>{{slaStr}}</dd>
        <dt>Previsão liberação</dt><dd>{{prevLibStr}}</dd>
        <dt>Previsão chegada terminal</dt><dd>{{prevTermStr}}</dd>
    </dl>

    <h2>Movimentação</h2>
    <dl class="grid">
        <dt>Descarregado no terminal</dt><dd>{{descTermStr}}</dd>
        <dt>Descarregado no pátio</dt><dd>{{descPatioStr}}</dd>
        <dt>Carregado no veículo</dt><dd>{{carrVeicStr}}</dd>
        <dt>Placa veículo</dt><dd>{{placaEsc}}</dd>
    </dl>

    <h2>Teste periódico e reparo</h2>
    <dl class="grid">
        <dt>Teste periódico (vencimento)</dt><dd>{{testPerStr}}</dd>
        <dt>Reparo acumulado (R$)</dt><dd>{{reparoStr}}</dd>
    </dl>

    <h2>Fotos da vistoria</h2>
    <div class="fotos">{{fotosHtml}}</div>

    <h2>Documentos</h2>
    {{certHtml}}
    {{laudoHtml}}

    <p style="margin-top: 24px;"><a href="/painel">← Voltar ao Dashboard</a></p>
</body>
</html>
""";
            return Results.Content(html, "text/html");
        });
    }
}
