using System.ComponentModel.DataAnnotations;

namespace PortalCliente.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Manter conectado")]
    public bool LembrarMe { get; set; }
}

public class CadastroUsuarioViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem.")]
    [Display(Name = "Confirmar senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    /// <summary>
    /// Role do usuário: "Admin" ou "Cliente".
    /// </summary>
    [Required(ErrorMessage = "Selecione o perfil.")]
    [Display(Name = "Perfil")]
    public string Role { get; set; } = "Cliente";

    /// <summary>
    /// Nome do cliente vinculado (obrigatório quando Role == "Cliente").
    /// Deve bater com o campo Cliente nos isotanques.
    /// </summary>
    [Display(Name = "Cliente vinculado")]
    public string? ClienteNome { get; set; }
}

/// <summary>ViewModel para edição de usuário pelo Admin.</summary>
public class EditarUsuarioViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome completo.")]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o perfil.")]
    [Display(Name = "Perfil")]
    public string Role { get; set; } = "Cliente";

    [Display(Name = "Cliente vinculado")]
    public string? ClienteNome { get; set; }
}

/// <summary>ViewModel para admin redefinir senha de um usuário.</summary>
public class RedefinirSenhaViewModel
{
    public string Id { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não coincidem.")]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

/// <summary>ViewModel para "Esqueci minha senha" — apenas o e-mail para envio do link.</summary>
public class EsqueciMinhaSenhaViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>ViewModel para redefinir senha via link recebido por e-mail.</summary>
public class RedefinirSenhaViaLinkViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não coincidem.")]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

/// <summary>ViewModel para o usuário alterar a própria senha.</summary>
public class AlterarMinhaSenhaViewModel
{
    [Required(ErrorMessage = "Informe a senha atual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não coincidem.")]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}
