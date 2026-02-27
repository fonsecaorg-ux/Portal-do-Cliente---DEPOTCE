using PortalCliente.Models;

namespace PortalCliente.Services;

/// <summary>
/// Serviço de leitura da configuração de contato (e-mail/WhatsApp/SMTP).
/// </summary>
public interface IConfiguracaoContatoService
{
    /// <summary>Obtém a configuração atual (geralmente a única linha, Id = 1).</summary>
    Task<ConfiguracaoContato?> GetAsync();

    /// <summary>Retorna a senha SMTP em texto puro para uso no envio (pode ser ofuscada no banco).</summary>
    Task<string?> GetSmtpPasswordPlainAsync();
}
