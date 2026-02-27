using Microsoft.EntityFrameworkCore;
using SimuladorMBM.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MbmDbContext>(options =>
    options.UseSqlite("Data Source=mbm.db"));

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MbmDbContext>();
    db.Database.EnsureCreated();
    await SeedData.EnsureSeedAsync(db);
    await SeedData.EnsureDocumentUrlsAsync(db);
}

app.UseCors();
app.UseStaticFiles();
app.MapControllers();

// Página inicial: explicação + link para o painel
app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Simulador MBM</title>
    <style>
        body { font-family: Segoe UI, sans-serif; max-width: 600px; margin: 40px auto; padding: 20px; }
        h1 { color: #0d6efd; }
        .ok { color: #198754; font-weight: bold; }
        .btn { display: inline-block; margin-top: 16px; padding: 10px 20px; background: #0d6efd; color: white; text-decoration: none; border-radius: 6px; }
        .btn:hover { background: #0b5ed7; }
        ul { line-height: 1.8; }
        a { color: #0d6efd; }
    </style>
</head>
<body>
    <h1>Simulador MBM – API</h1>
    <p class="ok">✓ API rodando corretamente.</p>
    <p>O MBM é o <strong>sistema base</strong>: aqui ficam os dados (isotanques, clientes, status). Quem mostra dashboards e telas para o cliente é o <strong>Portal</strong> (outra aplicação que consome esta API).</p>
    <p><a href="/painel" class="btn">Ver painel com os dados do MBM</a></p>
    <p>Endpoints da API:</p>
    <ul>
        <li><a href="/api/isotanques">/api/isotanques</a> – lista de isotanques</li>
        <li><a href="/api/clientes">/api/clientes</a> – lista de clientes</li>
        <li><a href="/api/status">/api/status</a> – lista de status</li>
    </ul>
    <p>Próximo passo: rode o <strong>Portal do Cliente</strong> em outro terminal e acesse <strong>http://localhost:5187</strong> para ver as telas que o cliente usa.</p>
</body>
</html>
""", "text/html"));

// Painel: resumo visual dos dados que o MBM guarda (para você “ver” o que é o MBM)
app.MapGet("/painel", async (MbmDbContext db) =>
{
    var totalIsotanques = await db.Isotanques.CountAsync();
    var totalClientes = await db.Clientes.CountAsync();
    var totalProdutos = await db.Produtos.CountAsync();
    var porStatus = await db.Isotanques
        .GroupBy(i => i.Status)
        .Select(g => new { Status = g.Key, Total = g.Count() })
        .OrderByDescending(x => x.Total)
        .ToListAsync();
    var amostra = await db.Isotanques
        .Include(i => i.Cliente)
        .Include(i => i.Produto)
        .OrderBy(i => i.Codigo)
        .Take(15)
        .Select(i => new { i.Codigo, Produto = i.Produto.Nome, Cliente = i.Cliente.Nome, i.Status, i.PrevisaoLiberacao })
        .ToListAsync();

    var linhasStatus = string.Join("", porStatus.Select(s => $@"<tr><td>{System.Net.WebUtility.HtmlEncode(s.Status)}</td><td>{s.Total}</td></tr>"));
    var linhasTab = string.Join("", amostra.Select(a => $@"<tr>
        <td>{System.Net.WebUtility.HtmlEncode(a.Codigo)}</td>
        <td>{System.Net.WebUtility.HtmlEncode(a.Produto)}</td>
        <td>{System.Net.WebUtility.HtmlEncode(a.Cliente)}</td>
        <td>{System.Net.WebUtility.HtmlEncode(a.Status)}</td>
        <td>{(a.PrevisaoLiberacao?.ToString("dd/MM/yyyy") ?? "–")}</td>
    </tr>"));

    var html = $$"""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Painel – Simulador MBM</title>
    <style>
        body { font-family: Segoe UI, sans-serif; max-width: 900px; margin: 20px auto; padding: 20px; }
        h1 { color: #0d6efd; }
        h2 { margin-top: 24px; font-size: 1.1rem; color: #333; }
        .cards { display: flex; gap: 16px; flex-wrap: wrap; margin: 16px 0; }
        .card { background: #f0f4ff; border: 1px solid #0d6efd; border-radius: 8px; padding: 16px 24px; min-width: 120px; }
        .card strong { display: block; font-size: 1.8rem; color: #0d6efd; }
        table { width: 100%; border-collapse: collapse; margin-top: 8px; }
        th, td { border: 1px solid #ddd; padding: 8px 10px; text-align: left; }
        th { background: #0d6efd; color: white; }
        tr:nth-child(even) { background: #f9f9f9; }
        a { color: #0d6efd; }
    </style>
</head>
<body>
    <h1>Painel – dados do MBM</h1>
    <p>Estes são os dados que o <strong>sistema base (MBM)</strong> guarda. O <strong>Portal do Cliente</strong> consome essa mesma informação e mostra em telas para o cliente.</p>
    <p><a href="/">← Voltar à página inicial</a></p>

    <h2>Resumo</h2>
    <div class="cards">
        <div class="card"><strong>{{totalIsotanques}}</strong> isotanques</div>
        <div class="card"><strong>{{totalClientes}}</strong> clientes</div>
        <div class="card"><strong>{{totalProdutos}}</strong> produtos</div>
    </div>

    <h2>Isotanques por status</h2>
    <table><thead><tr><th>Status</th><th>Quantidade</th></tr></thead><tbody>{{linhasStatus}}</tbody></table>

    <h2>Amostra de isotanques (primeiros 15)</h2>
    <table><thead><tr><th>Código</th><th>Produto</th><th>Cliente</th><th>Status</th><th>Previsão liberação</th></tr></thead><tbody>{{linhasTab}}</tbody></table>
</body>
</html>
""";
    return Results.Content(html, "text/html");
});

app.Run();
