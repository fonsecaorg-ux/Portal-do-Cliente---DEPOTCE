using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalCliente.Data;
using PortalCliente.Models;

namespace PortalCliente.Controllers;

[Authorize(Roles = "Admin")]
public class ConfiguracaoController : Controller
{
    private const int IdConfigPadrao = 1;
    private readonly AppDbContext _db;

    public ConfiguracaoController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Configuração de e-mail/WhatsApp/SMTP para o portal (Fale conosco).</summary>
    [HttpGet]
    public async Task<IActionResult> Contato(CancellationToken cancellationToken = default)
    {
        var config = await _db.ConfiguracoesContato
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == IdConfigPadrao, cancellationToken);

        if (config == null)
        {
            config = new ConfiguracaoContato { Id = IdConfigPadrao, SmtpPort = 587, SmtpEnableSsl = true };
        }
        else
        {
            config = new ConfiguracaoContato
            {
                Id = config.Id,
                EmailDestino = config.EmailDestino,
                WhatsAppNumero = config.WhatsAppNumero,
                NomeEquipe = config.NomeEquipe,
                SmtpHost = config.SmtpHost,
                SmtpPort = config.SmtpPort,
                SmtpEnableSsl = config.SmtpEnableSsl,
                SmtpUser = config.SmtpUser,
                SmtpPassword = null,
                FromEmail = config.FromEmail,
                FromName = config.FromName
            };
        }

        return View(config);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contato(ConfiguracaoContato model, CancellationToken cancellationToken = default)
    {
        model.Id = IdConfigPadrao;

        var existente = await _db.ConfiguracoesContato.FindAsync(new object[] { IdConfigPadrao }, cancellationToken);
        if (existente == null)
        {
            _db.ConfiguracoesContato.Add(model);
        }
        else
        {
            existente.EmailDestino = model.EmailDestino;
            existente.WhatsAppNumero = model.WhatsAppNumero;
            existente.NomeEquipe = model.NomeEquipe;
            existente.SmtpHost = model.SmtpHost;
            existente.SmtpPort = model.SmtpPort;
            existente.SmtpEnableSsl = model.SmtpEnableSsl;
            existente.SmtpUser = model.SmtpUser;
            if (!string.IsNullOrEmpty(model.SmtpPassword))
                existente.SmtpPassword = model.SmtpPassword;
            existente.FromEmail = model.FromEmail;
            existente.FromName = model.FromName;
        }

        await _db.SaveChangesAsync(cancellationToken);
        TempData["ConfigSucesso"] = "Configurações de contato salvas com sucesso.";
        return RedirectToAction(nameof(Contato));
    }
}
