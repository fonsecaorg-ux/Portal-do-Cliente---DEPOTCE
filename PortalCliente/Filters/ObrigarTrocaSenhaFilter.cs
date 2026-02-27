using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalCliente.Data;

namespace PortalCliente.Filters;

/// <summary>
/// Redireciona usuários que possuem a claim "trocar senha no próximo login" para a tela de alterar senha,
/// exceto quando já estão nela ou no logout.
/// </summary>
public class ObrigarTrocaSenhaFilter : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (!context.HttpContext.User.HasClaim(IdentitySeedData.ClaimMustChangePassword, "true"))
            return Task.CompletedTask;

        var controller = context.RouteData.Values["controller"] as string;
        var action = context.RouteData.Values["action"] as string;

        if (string.Equals(controller, "Conta", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(action, "AlterarMinhaSenha", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(action, "Logout", StringComparison.OrdinalIgnoreCase)))
            return Task.CompletedTask;

        context.Result = new RedirectToActionResult("AlterarMinhaSenha", "Conta", new { obrigatorio = true });
        return Task.CompletedTask;
    }
}
