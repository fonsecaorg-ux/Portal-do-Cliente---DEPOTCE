using Microsoft.EntityFrameworkCore;
using SimuladorMBM.Models;

namespace SimuladorMBM.Data;

/// <summary>
/// Carga inicial do MBM simulado: clientes, produtos, status e isotanques.
/// Mesmos dados lógicos do seed do Portal, com modelo normalizado.
/// </summary>
public static class SeedData
{
    public static async Task EnsureSeedAsync(MbmDbContext db)
    {
        if (await db.Isotanques.AnyAsync())
            return;

        var statusList = new[]
        {
            new StatusIsotanque { Codigo = "AG_OFF_HIRE", Descricao = "Ag. Off Hire", Ordem = 1 },
            new StatusIsotanque { Codigo = "AG_ENVIO_ESTIMATIVA", Descricao = "Ag. Envio Estimativa", Ordem = 2 },
            new StatusIsotanque { Codigo = "AG_LIMPEZA", Descricao = "Ag. Limpeza", Ordem = 3 },
            new StatusIsotanque { Codigo = "AG_REPARO", Descricao = "Ag. Reparo", Ordem = 4 },
            new StatusIsotanque { Codigo = "AG_INSPECAO", Descricao = "Ag. Inspeção", Ordem = 5 },
        };
        await db.StatusIsotanques.AddRangeAsync(statusList);
        await db.SaveChangesAsync();

        var clientes = new[]
        {
            new Cliente { Codigo = "DH", Nome = "DEN HARTOGH", EmailContato = "contato@denhartogh.com" },
            new Cliente { Codigo = "EA", Nome = "Empresa Alpha", EmailContato = "logistica@empresaalpha.com" },
            new Cliente { Codigo = "QB", Nome = "Química Beta", EmailContato = "operacoes@quimicabeta.com" },
        };
        await db.Clientes.AddRangeAsync(clientes);
        await db.SaveChangesAsync();

        var produtos = new[]
        {
            new Produto { Codigo = "ET", Nome = "Etanol", ClasseRisco = "3" },
            new Produto { Codigo = "MT", Nome = "Metanol", ClasseRisco = "3" },
            new Produto { Codigo = "AC", Nome = "Ácido acético", ClasseRisco = "8" },
            new Produto { Codigo = "TL", Nome = "Tolueno", ClasseRisco = "3" },
            new Produto { Codigo = "PG", Nome = "Propilenoglicol", ClasseRisco = "0" },
            new Produto { Codigo = "HX", Nome = "Hexano", ClasseRisco = "3" },
        };
        await db.Produtos.AddRangeAsync(produtos);
        await db.SaveChangesAsync();

        var hoje = DateTime.Today;
        var dh = await db.Clientes.FirstAsync(c => c.Codigo == "DH");
        var ea = await db.Clientes.FirstAsync(c => c.Codigo == "EA");
        var qb = await db.Clientes.FirstAsync(c => c.Codigo == "QB");
        var etanol = await db.Produtos.FirstAsync(p => p.Codigo == "ET");
        var metanol = await db.Produtos.FirstAsync(p => p.Codigo == "MT");
        var acido = await db.Produtos.FirstAsync(p => p.Codigo == "AC");
        var tolueno = await db.Produtos.FirstAsync(p => p.Codigo == "TL");
        var propilenoglicol = await db.Produtos.FirstAsync(p => p.Codigo == "PG");
        var hexano = await db.Produtos.FirstAsync(p => p.Codigo == "HX");

        var agora = DateTime.Now;
        // DataInicioStatus = quando entrou no status atual → "Dias no Status" (como no BI "Isotank Vazio - Dias no Status")
        var isotanques = new List<Isotanque>
        {
            new() { Codigo = "DHDU1274480", ProdutoId = etanol.Id, ClienteId = dh.Id, Status = "Ag. Envio Estimativa", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(2), DataEntrada = hoje.AddDays(-5), UltimaAtualizacao = hoje, DataHoraDescarregadoPatio = agora.AddDays(-5).AddHours(8), UrlFoto = "https://placehold.co/400x200/0d6efd/white?text=Isotanque+DHDU1274480", NumeroBooking = "BK-2026-001" },
            new() { Codigo = "DHDL2272373", ProdutoId = metanol.Id, ClienteId = dh.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(5), DataEntrada = hoje.AddDays(-3), UltimaAtualizacao = hoje, DataHoraDescarregadoPatio = agora.AddDays(-3).AddHours(14), DataHoraCarregadoVeiculo = agora.AddDays(-1).AddHours(9), DataSaida = hoje.AddDays(1), PrevisaoChegadaTerminal = hoje.AddDays(1).AddHours(16), UrlFoto = "https://placehold.co/400x200/198754/white?text=Isotanque+DHDL2272373", NumeroBooking = "BK-2026-001" },
            new() { Codigo = "DHDU2273512", ProdutoId = acido.Id, ClienteId = dh.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(3), DataEntrada = hoje.AddDays(-2), UltimaAtualizacao = hoje, DataHoraDescarregadoPatio = agora.AddDays(-2).AddHours(7), UrlFoto = "https://placehold.co/400x200/6f42c1/white?text=Isotanque+DHDU2273512" },
            new() { Codigo = "DHDL2413363", ProdutoId = tolueno.Id, ClienteId = dh.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje.AddDays(-1), PrevisaoLiberacao = hoje.AddDays(7), DataEntrada = hoje.AddDays(-10), UltimaAtualizacao = hoje, DataHoraDescarregadoPatio = agora.AddDays(-10).AddHours(11) },
            new() { Codigo = "DHDU1971099", ProdutoId = etanol.Id, ClienteId = dh.Id, Status = "Ag. Inspeção", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(1), DataEntrada = hoje.AddDays(-1), UltimaAtualizacao = hoje, DataHoraDescarregadoPatio = agora.AddDays(-1).AddHours(6), DataHoraCarregadoVeiculo = agora.AddHours(-2), DataSaida = hoje.AddDays(1), PrevisaoChegadaTerminal = hoje.AddDays(1).AddHours(8), UrlFoto = "https://placehold.co/400x200/dc3545/white?text=Isotanque+DHDU1971099" },
            new() { Codigo = "DHDU3012345", ProdutoId = metanol.Id, ClienteId = dh.Id, Status = "Ag. Inspeção", DataInicioStatus = hoje.AddDays(-7), PrevisaoLiberacao = hoje.AddDays(8), DataEntrada = hoje.AddDays(-7), UltimaAtualizacao = hoje, DataHoraDescarregadoPatio = agora.AddDays(-7).AddHours(9) },
            new() { Codigo = "DHDL3399881", ProdutoId = acido.Id, ClienteId = dh.Id, Status = "Ag. Reparo", DataInicioStatus = hoje.AddDays(-5), PrevisaoLiberacao = hoje.AddDays(12), DataEntrada = hoje.AddDays(-15), UltimaAtualizacao = hoje },
            new() { Codigo = "DHDU4455667", ProdutoId = tolueno.Id, ClienteId = dh.Id, Status = "Ag. Envio Estimativa", DataInicioStatus = hoje.AddDays(-2), PrevisaoLiberacao = hoje.AddDays(5), DataEntrada = hoje.AddDays(-4), UltimaAtualizacao = hoje },
            new() { Codigo = "DHDL1122334", ProdutoId = propilenoglicol.Id, ClienteId = dh.Id, Status = "Ag. Inspeção", DataInicioStatus = hoje.AddDays(-3), PrevisaoLiberacao = hoje.AddDays(6), DataEntrada = hoje.AddDays(-6), UltimaAtualizacao = hoje },
            new() { Codigo = "DHDU3344556", ProdutoId = acido.Id, ClienteId = dh.Id, Status = "Ag. Envio Estimativa", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(4), DataEntrada = hoje.AddDays(-2), UltimaAtualizacao = hoje },
            new() { Codigo = "DHDL6677889", ProdutoId = metanol.Id, ClienteId = dh.Id, Status = "Ag. Reparo", DataInicioStatus = hoje.AddDays(-20), PrevisaoLiberacao = hoje.AddDays(15), DataEntrada = hoje.AddDays(-20), UltimaAtualizacao = hoje },
            new() { Codigo = "DHDU8899001", ProdutoId = etanol.Id, ClienteId = dh.Id, Status = "Ag. Off Hire", DataInicioStatus = hoje.AddDays(-30), PrevisaoLiberacao = null, DataEntrada = hoje.AddDays(-30), DataSaida = hoje.AddDays(-5), UltimaAtualizacao = hoje },
            new() { Codigo = "EXFU5567363", ProdutoId = etanol.Id, ClienteId = ea.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(1), DataEntrada = hoje.AddDays(-1), UltimaAtualizacao = hoje, NumeroBooking = "BK-2026-002" },
            new() { Codigo = "EXFU6422402", ProdutoId = metanol.Id, ClienteId = ea.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje.AddDays(-2), PrevisaoLiberacao = hoje.AddDays(4), DataEntrada = hoje.AddDays(-3), UltimaAtualizacao = hoje, NumeroBooking = "BK-2026-002" },
            new() { Codigo = "EXFU6632144", ProdutoId = tolueno.Id, ClienteId = ea.Id, Status = "Ag. Inspeção", DataInicioStatus = hoje.AddDays(-14), PrevisaoLiberacao = hoje.AddDays(14), DataEntrada = hoje.AddDays(-12), UltimaAtualizacao = hoje },
            new() { Codigo = "EXFU7711223", ProdutoId = hexano.Id, ClienteId = ea.Id, Status = "Ag. Reparo", DataInicioStatus = hoje.AddDays(-10), PrevisaoLiberacao = hoje.AddDays(21), DataEntrada = hoje.AddDays(-25), UltimaAtualizacao = hoje },
            new() { Codigo = "EXFU9988776", ProdutoId = metanol.Id, ClienteId = ea.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(2), DataEntrada = hoje.AddDays(-2), UltimaAtualizacao = hoje },
            new() { Codigo = "EXFU5566778", ProdutoId = tolueno.Id, ClienteId = ea.Id, Status = "Ag. Inspeção", DataInicioStatus = hoje.AddDays(-8), PrevisaoLiberacao = hoje.AddDays(10), DataEntrada = hoje.AddDays(-8), UltimaAtualizacao = hoje },
            new() { Codigo = "EXFU9900112", ProdutoId = tolueno.Id, ClienteId = ea.Id, Status = "Ag. Envio Estimativa", DataInicioStatus = hoje.AddDays(-5), PrevisaoLiberacao = hoje.AddDays(7), DataEntrada = hoje.AddDays(-5), UltimaAtualizacao = hoje },
            new() { Codigo = "SEDU8063278", ProdutoId = acido.Id, ClienteId = qb.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje, PrevisaoLiberacao = null, DataEntrada = hoje.AddDays(-1), UltimaAtualizacao = hoje, NumeroBooking = "BK-2026-003" },
            new() { Codigo = "SEDU5544332", ProdutoId = etanol.Id, ClienteId = qb.Id, Status = "Ag. Off Hire", DataInicioStatus = hoje.AddDays(-45), PrevisaoLiberacao = null, DataEntrada = hoje.AddDays(-45), DataSaida = hoje.AddDays(-10), UltimaAtualizacao = hoje },
            new() { Codigo = "SEDU2233445", ProdutoId = etanol.Id, ClienteId = qb.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje.AddDays(-1), PrevisaoLiberacao = hoje.AddDays(3), DataEntrada = hoje.AddDays(-2), UltimaAtualizacao = hoje },
            new() { Codigo = "SEDU7788990", ProdutoId = hexano.Id, ClienteId = qb.Id, Status = "Ag. Limpeza", DataInicioStatus = hoje, PrevisaoLiberacao = hoje.AddDays(1), DataEntrada = hoje.AddDays(-1), UltimaAtualizacao = hoje },
        };

        await db.Isotanques.AddRangeAsync(isotanques);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Garante colunas de documentos e Painel no banco (se já existir) e atualiza URLs dos laudos/certificados de exemplo.
    /// </summary>
    public static async Task EnsureDocumentUrlsAsync(MbmDbContext db)
    {
        // Só adiciona colunas que ainda não existem (evita FAIL no log quando o banco já está atualizado)
        var colunasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(Isotanques)";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                colunasExistentes.Add(reader.GetString(1)); // name
        }
        conn.Close();

        var colunasNovas = new[] { ("UrlLaudoVistoria", "TEXT"), ("UrlCertificadoLavagem", "TEXT"), ("DataHoraDescarregadoTerminal", "TEXT"), ("Tipo", "TEXT"), ("ProprietarioArmador", "TEXT"), ("PlacaVeiculo", "TEXT"), ("SlaLimite", "TEXT"), ("TestePeriodicoVencimento", "TEXT"), ("ReparoAcumuladoValor", "REAL"), ("UrlsFotos", "TEXT"), ("Patio", "TEXT"), ("Bloco", "TEXT"), ("Fila", "TEXT"), ("Pilha", "TEXT"), ("NumeroBooking", "TEXT"), ("DataSaida", "TEXT") };
        foreach (var (nome, tipo) in colunasNovas)
        {
            if (colunasExistentes.Contains(nome))
                continue;
            await db.Database.ExecuteSqlRawAsync($"ALTER TABLE Isotanques ADD COLUMN {nome} {tipo}");
        }

        var eirCodigos = new[] { "DHDU1274480", "DHDL2272373", "DHDU2273512", "DHDL3399881", "DHDU4455667", "EXFU5567363", "EXFU6422402", "EXFU7711223", "SEDU2233445", "SEDU7788990" };
        var certCodigos = new[] { "DHDU1274480", "DHDL2272373", "DHDU2273512", "DHDL3399881", "DHDU4455667", "EXFU5567363", "EXFU6422402", "EXFU7711223", "SEDU2233445" };

        foreach (var codigo in eirCodigos)
        {
            var ent = await db.Isotanques.FirstOrDefaultAsync(i => i.Codigo == codigo);
            if (ent != null)
            {
                ent.UrlLaudoVistoria = "/docs/laudos/" + codigo + "_EIR.pdf";
                if (certCodigos.Contains(codigo))
                    ent.UrlCertificadoLavagem = "/docs/certificados/" + codigo + "_CleaningCertificate.pdf";
            }
        }

        foreach (var codigo in certCodigos)
        {
            var ent = await db.Isotanques.FirstOrDefaultAsync(i => i.Codigo == codigo);
            if (ent != null && string.IsNullOrEmpty(ent.UrlCertificadoLavagem))
                ent.UrlCertificadoLavagem = "/docs/certificados/" + codigo + "_CleaningCertificate.pdf";
        }

        // Foto de exemplo do isotank DHDL1122334 (Propilenoglicol / DEN HARTOGH) — coloque DHDL1122334.png em wwwroot/docs/fotos/
        var fotoEnt = await db.Isotanques.FirstOrDefaultAsync(i => i.Codigo == "DHDL1122334");
        if (fotoEnt != null)
            fotoEnt.UrlFoto = "/docs/fotos/DHDL1122334.png";

        await db.SaveChangesAsync();
    }
}
