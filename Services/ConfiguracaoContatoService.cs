using Microsoft.EntityFrameworkCore;
using PortalCliente.Data;
using PortalCliente.Models;

namespace PortalCliente.Services;

/// <summary>
/// Implementação que lê a configuração de contato do banco (tabela ConfiguracoesContato).
/// </summary>
public class ConfiguracaoContatoService : IConfiguracaoContatoService
{
    private const int IdConfigPadrao = 1;
    private readonly AppDbContext _db;

    public ConfiguracaoContatoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ConfiguracaoContato?> GetAsync()
    {
        return await _db.ConfiguracoesContato
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == IdConfigPadrao);
    }

    public Task<string?> GetSmtpPasswordPlainAsync()
    {
        // Por simplicidade, a senha é armazenada em texto; se no futuro for ofuscada, desofuscar aqui.
        return _db.ConfiguracoesContato
            .AsNoTracking()
            .Where(c => c.Id == IdConfigPadrao)
            .Select(c => c.SmtpPassword)
            .FirstOrDefaultAsync();
    }
}
