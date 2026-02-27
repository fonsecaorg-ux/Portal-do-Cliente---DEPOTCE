using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortalCliente.Models;
using PortalCliente.Services;

namespace PortalCliente.ViewComponents;

/// <summary>
/// Badge no menu com a quantidade de isotanks em situação de alerta (parados ≥10 dias ou previsão até 3 dias).
/// </summary>
public class AlertasBadgeViewComponent : ViewComponent
{
    private readonly IOrigenDadosService _origen;
    private readonly UserManager<UsuarioAplicacao> _userManager;

    public AlertasBadgeViewComponent(IOrigenDadosService origen, UserManager<UsuarioAplicacao> userManager)
    {
        _origen = origen;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var ehAdmin = HttpContext.User.IsInRole("Admin");
        var cliente = ehAdmin ? null : user?.ClienteNome;

        var todos = await _origen.GetIsotanquesAsync(cliente, null, null);
        var hoje = DateTime.Today;
        var limitePrevisao = hoje.AddDays(3);

        var parados = todos.Where(c => (c.DiasNoStatus ?? 0) >= 10).Select(c => c.Codigo).ToHashSet();
        var previsoes = todos
            .Where(c => c.PrevisaoLiberacao.HasValue && c.PrevisaoLiberacao.Value.Date <= limitePrevisao)
            .Select(c => c.Codigo)
            .ToHashSet();
        parados.UnionWith(previsoes);
        var total = parados.Count;

        return View(total);
    }
}
