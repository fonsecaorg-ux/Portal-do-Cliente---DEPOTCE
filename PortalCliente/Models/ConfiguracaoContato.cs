namespace PortalCliente.Models;

/// <summary>
/// Configuração única de contato (e-mail/WhatsApp/SMTP) para o portal.
/// Normalmente uma única linha (Id = 1) configurada por um admin.
/// </summary>
public class ConfiguracaoContato
{
    public int Id { get; set; }

    public string? EmailDestino { get; set; }
    public string? WhatsAppNumero { get; set; }
    public string? NomeEquipe { get; set; }

    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool SmtpEnableSsl { get; set; } = true;
    public string? SmtpUser { get; set; }
    /// <summary>Senha SMTP (pode ser armazenada em texto ou ofuscada; o serviço expõe em texto para envio).</summary>
    public string? SmtpPassword { get; set; }

    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}
