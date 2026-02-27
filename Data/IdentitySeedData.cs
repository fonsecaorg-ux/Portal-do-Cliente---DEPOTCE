using Microsoft.AspNetCore.Identity;
using PortalCliente.Models;

namespace PortalCliente.Data;

/// <summary>
/// Cria as roles padrão e o primeiro usuário admin na primeira execução.
/// </summary>
public static class IdentitySeedData
{
    public const string RoleAdmin = "Admin";
    public const string RoleCliente = "Cliente";

    /// <summary>Claim que obriga o usuário a alterar a senha no próximo login (após admin redefinir).</summary>
    public const string ClaimMustChangePassword = "PortalCliente.MustChangePassword";

    // Credenciais do admin inicial — altere antes de colocar em produção
    private const string AdminEmail = "admin@depotce.com.br";
    private const string AdminSenha = "Depotce@2026";
    private const string AdminNome = "Administrador Depotce";

    public static async Task EnsureSeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<UsuarioAplicacao> userManager)
    {
        // Criar roles
        foreach (var role in new[] { RoleAdmin, RoleCliente })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Criar admin inicial se não existir
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin == null)
        {
            admin = new UsuarioAplicacao
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                NomeCompleto = AdminNome,
                ClienteNome = null, // admin não está vinculado a um cliente
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, AdminSenha);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, RoleAdmin);
        }
    }
}
