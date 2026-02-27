using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortalCliente.Data;
using PortalCliente.Models;
using PortalCliente.Services;

namespace PortalCliente.Controllers;

public class ContaController : Controller
{
    private readonly SignInManager<UsuarioAplicacao> _signIn;
    private readonly UserManager<UsuarioAplicacao> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrigenDadosService _origen;
    private readonly IConfiguration _configuration;
    private readonly IConfiguracaoContatoService _configContato;

    public ContaController(
        SignInManager<UsuarioAplicacao> signIn,
        UserManager<UsuarioAplicacao> userManager,
        RoleManager<IdentityRole> roleManager,
        IOrigenDadosService origen,
        IConfiguration configuration,
        IConfiguracaoContatoService configContato)
    {
        _signIn = signIn;
        _userManager = userManager;
        _roleManager = roleManager;
        _origen = origen;
        _configuration = configuration;
        _configContato = configContato;
    }

    // ──────────────────────────────────────────────
    // LOGIN
    // ──────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signIn.PasswordSignInAsync(
            model.Email,
            model.Senha,
            model.LembrarMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var usuario = await _userManager.FindByEmailAsync(model.Email);
            if (usuario != null && (await _userManager.GetClaimsAsync(usuario)).Any(c => c.Type == IdentitySeedData.ClaimMustChangePassword && c.Value == "true"))
            {
                TempData["AlterarSenhaObrigatorio"] = true;
                return RedirectToAction(nameof(AlterarMinhaSenha), new { obrigatorio = true });
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
        return View(model);
    }

    // ──────────────────────────────────────────────
    // LOGOUT
    // ──────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    // ──────────────────────────────────────────────
    // ESQUECI MINHA SENHA (envio de link por e-mail)
    // ──────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult EsqueciMinhaSenha()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new EsqueciMinhaSenhaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> EsqueciMinhaSenha(EsqueciMinhaSenhaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _userManager.FindByEmailAsync(model.Email?.Trim() ?? "");
        if (usuario == null)
        {
            // Não revelar se o e-mail existe ou não (segurança)
            TempData["EsqueciMinhaSenhaSucesso"] = "Se esse e-mail estiver cadastrado, você receberá um link para redefinir sua senha. Verifique sua caixa de entrada e o spam.";
            return RedirectToAction(nameof(EsqueciMinhaSenha));
        }

        if (await _userManager.IsLockedOutAsync(usuario))
        {
            TempData["EsqueciMinhaSenhaErro"] = "Esta conta está desabilitada. Entre em contato com a equipe Depotce.";
            return RedirectToAction(nameof(EsqueciMinhaSenha));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var callbackUrl = Url.Action(
            nameof(RedefinirSenhaViaLink),
            "Conta",
            new { userId = usuario.Id, token }, // token será codificado na URL
            Request.Scheme,
            Request.Host.Value);

        var nomeUsuario = usuario.NomeCompleto?.Trim() ?? usuario.Email ?? "Cliente";
        var enviado = await EnviarEmailRedefinicaoSenhaAsync(usuario.Email!, callbackUrl, nomeUsuario);
        if (!enviado)
        {
            TempData["EsqueciMinhaSenhaErro"] = "Não foi possível enviar o e-mail. Verifique se o SMTP está configurado em Configurações de contato ou use a opção \"Fale com a Depotce\" para solicitar redefinição.";
            return View(model);
        }

        TempData["EsqueciMinhaSenhaSucesso"] = "Se esse e-mail estiver cadastrado, você receberá um link para redefinir sua senha. Verifique sua caixa de entrada e o spam.";
        return RedirectToAction(nameof(EsqueciMinhaSenha));
    }

    /// <summary>Página para definir nova senha ao clicar no link recebido por e-mail.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> RedefinirSenhaViaLink(string? userId, string? token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            TempData["RedefinirSenhaErro"] = "Link inválido ou expirado. Solicite um novo link em \"Esqueci minha senha\".";
            return RedirectToAction(nameof(Login));
        }

        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario == null)
        {
            TempData["RedefinirSenhaErro"] = "Link inválido ou expirado. Solicite um novo link em \"Esqueci minha senha\".";
            return RedirectToAction(nameof(Login));
        }

        // Alguns clientes de e-mail convertem '+' em espaço na URL; o token do Identity pode conter '+'
        var tokenCorrigido = (token ?? "").Replace(' ', '+');
        return View(new RedefinirSenhaViaLinkViewModel { UserId = userId, Token = tokenCorrigido });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> RedefinirSenhaViaLink(RedefinirSenhaViaLinkViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _userManager.FindByIdAsync(model.UserId);
        if (usuario == null)
        {
            TempData["RedefinirSenhaErro"] = "Link inválido ou expirado. Solicite um novo link em \"Esqueci minha senha\".";
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ResetPasswordAsync(usuario, model.Token, model.NovaSenha);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        TempData["RedefinirSenhaSucesso"] = "Sua senha foi redefinida. Faça login com a nova senha.";
        return RedirectToAction(nameof(Login));
    }

    /// <summary>Envia e-mail com link de redefinição de senha. Usa a mesma config SMTP do Contato.</summary>
    private async Task<bool> EnviarEmailRedefinicaoSenhaAsync(string destino, string callbackUrl, string nomeUsuario)
    {
        var config = await _configContato.GetAsync();
        var smtpHost = config?.SmtpHost?.Trim() ?? _configuration["Contato:SmtpHost"]?.Trim();
        var smtpUser = config?.SmtpUser?.Trim() ?? _configuration["Contato:SmtpUser"]?.Trim();
        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser))
            return false;

        var smtpPassword = !string.IsNullOrEmpty(config?.SmtpUser)
            ? await _configContato.GetSmtpPasswordPlainAsync()
            : _configuration["Contato:SmtpPassword"];
        var fromEmail = config?.FromEmail?.Trim() ?? _configuration["Contato:FromEmail"]?.Trim() ?? smtpUser;
        var fromName = config?.FromName?.Trim() ?? _configuration["Contato:FromName"]?.Trim() ?? "Portal do Cliente Depotce";

        var assunto = "[Portal Depotce] Redefinição de senha";
        var corpo = $"""
<!DOCTYPE html>
<html lang="pt-BR">
<head><meta charset="utf-8"/></head>
<body style="margin:0; padding:0; background:#f4f4f4; font-family: 'Segoe UI', Arial, sans-serif;">
  <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f4; padding:32px 0;">
    <tr><td align="center">
      <table width="560" cellpadding="0" cellspacing="0" style="background:white; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.08);">
        <tr>
          <td style="background:#8B1A1A; padding:28px 40px;">
            <span style="color:white; font-size:20px; font-weight:700;">Depotce</span>
            <span style="color:rgba(255,255,255,0.6); font-size:11px; letter-spacing:2px; text-transform:uppercase; margin-left:8px;">Portal do Cliente</span>
          </td>
        </tr>
        <tr><td style="background:#5a1010; height:4px;"></td></tr>
        <tr>
          <td style="padding:40px 40px 32px;">
            <div style="text-align:center; margin-bottom:24px; font-size:40px;">🔐</div>
            <h1 style="margin:0 0 8px; font-size:22px; color:#1a1a1a; text-align:center; font-weight:700;">Redefinição de senha</h1>
            <p style="margin:0 0 28px; font-size:13px; color:#999; text-align:center;">Portal do Cliente Depotce</p>
            <p style="margin:0 0 16px; font-size:15px; color:#333; line-height:1.6;">Olá, <strong>{nomeUsuario}</strong>!</p>
            <p style="margin:0 0 28px; font-size:15px; color:#555; line-height:1.7;">
              Recebemos uma solicitação para redefinir a senha da sua conta no <strong>Portal do Cliente Depotce</strong>. Clique no botão abaixo para criar uma nova senha:
            </p>
            <div style="text-align:center; margin:0 0 28px;">
              <a href="{callbackUrl}" style="display:inline-block; background:#8B1A1A; color:white; text-decoration:none; padding:14px 40px; border-radius:5px; font-size:14px; font-weight:700; letter-spacing:1.5px; text-transform:uppercase;">
                Redefinir minha senha
              </a>
            </div>
            <div style="background:#fff8f0; border-left:3px solid #fd7e14; border-radius:4px; padding:12px 16px; margin-bottom:28px;">
              <p style="margin:0; font-size:13px; color:#8a5c00;">
                ⏱ <strong>Atenção:</strong> Este link expira em <strong>2 horas</strong>. Após esse prazo, será necessário solicitar uma nova redefinição.
              </p>
            </div>
            <div style="border-top:1px solid #f0f0f0; padding-top:20px;">
              <p style="margin:0 0 8px; font-size:13px; color:#999; line-height:1.6;">
                🔒 Se você <strong>não solicitou</strong> a redefinição de senha, ignore este e-mail. Sua senha permanece a mesma e nenhuma alteração foi feita.
              </p>
            </div>
          </td>
        </tr>
        <tr>
          <td style="background:#f7f7f5; border-top:1px solid #eee; padding:20px 40px;">
            <p style="margin:0; font-size:11px; color:#aaa;">Equipe Depotce • Grupo Cesari — Portal do Cliente • Este é um e-mail automático, não responda.</p>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>
""";

        try
        {
            using var client = new SmtpClient(smtpHost)
            {
                Port = config?.SmtpPort ?? (int.TryParse(_configuration["Contato:SmtpPort"], out var p) ? p : 587),
                EnableSsl = config?.SmtpEnableSsl ?? string.Equals(_configuration["Contato:SmtpEnableSsl"], "true", StringComparison.OrdinalIgnoreCase),
                Credentials = new NetworkCredential(smtpUser, smtpPassword ?? "")
            };
            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = assunto,
                Body = corpo,
                IsBodyHtml = true
            };
            mail.To.Add(destino);
            await client.SendMailAsync(mail);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ──────────────────────────────────────────────
    // CADASTRO DE USUÁRIO (somente Admin)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> CadastrarUsuario()
    {
        var clientes = await _origen.GetClientesAsync();
        ViewBag.Clientes = clientes;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> CadastrarUsuario(CadastroUsuarioViewModel model)
    {
        // Validação extra: cliente vinculado obrigatório para role Cliente
        if (model.Role == IdentitySeedData.RoleCliente && string.IsNullOrWhiteSpace(model.ClienteNome))
            ModelState.AddModelError(nameof(model.ClienteNome), "Selecione o cliente vinculado.");

        if (!ModelState.IsValid)
        {
            ViewBag.Clientes = await _origen.GetClientesAsync();
            return View(model);
        }

        var usuario = new UsuarioAplicacao
        {
            UserName = model.Email,
            Email = model.Email,
            NomeCompleto = model.NomeCompleto,
            ClienteNome = model.Role == IdentitySeedData.RoleCliente ? model.ClienteNome : null,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(usuario, model.Senha);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Clientes = await _origen.GetClientesAsync();
            return View(model);
        }

        await _userManager.AddToRoleAsync(usuario, model.Role);

        TempData["Mensagem"] = $"Usuário '{model.Email}' cadastrado com sucesso.";
        return RedirectToAction(nameof(ListarUsuarios));
    }

    // ──────────────────────────────────────────────
    // LISTAGEM DE USUÁRIOS (somente Admin)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = _userManager.Users.OrderBy(u => u.NomeCompleto).ToList();
        var desabilitados = new HashSet<string>();
        foreach (var u in usuarios)
        {
            if (await _userManager.IsLockedOutAsync(u))
                desabilitados.Add(u.Id);
        }
        ViewBag.Desabilitados = desabilitados;
        return View(usuarios);
    }

    // ──────────────────────────────────────────────
    // EDITAR USUÁRIO (somente Admin)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> EditarUsuario(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(ListarUsuarios));
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return RedirectToAction(nameof(ListarUsuarios));

        var roles = await _userManager.GetRolesAsync(usuario);
        var role = roles.FirstOrDefault() ?? IdentitySeedData.RoleCliente;

        ViewBag.Clientes = await _origen.GetClientesAsync();
        return View(new EditarUsuarioViewModel
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto ?? "",
            Email = usuario.Email ?? "",
            Role = role,
            ClienteNome = usuario.ClienteNome
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> EditarUsuario(EditarUsuarioViewModel model)
    {
        if (model.Role == IdentitySeedData.RoleCliente && string.IsNullOrWhiteSpace(model.ClienteNome))
            ModelState.AddModelError(nameof(model.ClienteNome), "Selecione o cliente vinculado.");

        if (!ModelState.IsValid)
        {
            ViewBag.Clientes = await _origen.GetClientesAsync();
            return View(model);
        }

        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario == null)
        {
            TempData["Mensagem"] = "Usuário não encontrado.";
            return RedirectToAction(nameof(ListarUsuarios));
        }

        usuario.NomeCompleto = model.NomeCompleto;
        usuario.Email = model.Email;
        usuario.UserName = model.Email;
        usuario.ClienteNome = model.Role == IdentitySeedData.RoleCliente ? model.ClienteNome : null;

        var result = await _userManager.UpdateAsync(usuario);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            ViewBag.Clientes = await _origen.GetClientesAsync();
            return View(model);
        }

        var rolesAtuais = await _userManager.GetRolesAsync(usuario);
        var roleDesejada = model.Role;
        foreach (var r in rolesAtuais.Where(r => r != roleDesejada))
            await _userManager.RemoveFromRoleAsync(usuario, r);
        if (!rolesAtuais.Contains(roleDesejada))
            await _userManager.AddToRoleAsync(usuario, roleDesejada);

        TempData["Mensagem"] = "Usuário atualizado com sucesso.";
        return RedirectToAction(nameof(ListarUsuarios));
    }

    // ──────────────────────────────────────────────
    // REDEFINIR SENHA (Admin)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> RedefinirSenha(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(ListarUsuarios));
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return RedirectToAction(nameof(ListarUsuarios));

        return View(new RedefinirSenhaViewModel
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto ?? "",
            Email = usuario.Email ?? ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario == null)
        {
            TempData["Mensagem"] = "Usuário não encontrado.";
            return RedirectToAction(nameof(ListarUsuarios));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var result = await _userManager.ResetPasswordAsync(usuario, token, model.NovaSenha);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        // Obrigar troca de senha no próximo login (remove claim antiga se existir)
        var claimsAntigos = (await _userManager.GetClaimsAsync(usuario)).Where(c => c.Type == IdentitySeedData.ClaimMustChangePassword).ToList();
        foreach (var c in claimsAntigos)
            await _userManager.RemoveClaimAsync(usuario, c);
        await _userManager.AddClaimAsync(usuario, new Claim(IdentitySeedData.ClaimMustChangePassword, "true"));

        // Invalida o cookie de quem está logado com essa conta; no próximo login o usuário recebe a claim e é redirecionado para AlterarMinhaSenha
        await _userManager.UpdateSecurityStampAsync(usuario);

        TempData["Mensagem"] = "Senha redefinida. O usuário será obrigado a alterar a senha no próximo login.";
        return RedirectToAction(nameof(ListarUsuarios));
    }

    // ──────────────────────────────────────────────
    // DESABILITAR / REATIVAR USUÁRIO (Admin)
    // ──────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> DesabilitarUsuario(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(ListarUsuarios));
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null && currentUser.Id == id)
        {
            TempData["Mensagem"] = "Você não pode desabilitar sua própria conta.";
            return RedirectToAction(nameof(ListarUsuarios));
        }
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return RedirectToAction(nameof(ListarUsuarios));

        await _userManager.SetLockoutEnabledAsync(usuario, true);
        await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.UtcNow.AddYears(10));
        TempData["Mensagem"] = "Usuário desabilitado. Ele não poderá fazer login.";
        return RedirectToAction(nameof(ListarUsuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = IdentitySeedData.RoleAdmin)]
    public async Task<IActionResult> ReativarUsuario(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(ListarUsuarios));
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return RedirectToAction(nameof(ListarUsuarios));

        await _userManager.SetLockoutEndDateAsync(usuario, null);
        TempData["Mensagem"] = "Usuário reativado.";
        return RedirectToAction(nameof(ListarUsuarios));
    }

    // ──────────────────────────────────────────────
    // ALTERAR MINHA SENHA (qualquer usuário logado)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Authorize]
    public IActionResult AlterarMinhaSenha(bool? obrigatorio = null)
    {
        ViewBag.AlterarSenhaObrigatorio = obrigatorio == true || (TempData["AlterarSenhaObrigatorio"] as bool? ?? false);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AlterarMinhaSenha(AlterarMinhaSenhaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return RedirectToAction("Index", "Home");

        var result = await _userManager.ChangePasswordAsync(usuario, model.SenhaAtual, model.NovaSenha);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        // Remover claim de troca obrigatória e atualizar o cookie de login
        var claim = (await _userManager.GetClaimsAsync(usuario)).FirstOrDefault(c => c.Type == IdentitySeedData.ClaimMustChangePassword);
        if (claim != null)
            await _userManager.RemoveClaimAsync(usuario, claim);

        await _signIn.RefreshSignInAsync(usuario);
        TempData["Mensagem"] = "Sua senha foi alterada com sucesso.";
        return RedirectToAction("Index", "Home");
    }

    // ──────────────────────────────────────────────
    // ACESSO NEGADO
    // ──────────────────────────────────────────────

    public IActionResult AcessoNegado()
    {
        return View();
    }
}
