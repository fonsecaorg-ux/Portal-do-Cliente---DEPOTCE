using Microsoft.AspNetCore.Identity;

namespace PortalCliente.Models;

/// <summary>
/// Usuário do portal. Estende IdentityUser com vínculo ao cliente do MBM.
/// - Role "Admin": acessa todos os isotanques, vê dropdown de cliente.
/// - Role "Cliente": vê apenas os isotanques do seu ClienteNome.
/// </summary>
public class UsuarioAplicacao : IdentityUser
{
    /// <summary>
    /// Nome do cliente vinculado (deve bater com o campo Cliente nos isotanques).
    /// Null para admins da Depotce.
    /// </summary>
    public string? ClienteNome { get; set; }

    /// <summary>Nome de exibição no portal.</summary>
    public string NomeCompleto { get; set; } = string.Empty;
}
