namespace PortalCliente.Models;

/// <summary>
/// Observação interna sobre um isotank, registrada por usuário (ex.: Admin).
/// </summary>
public class ObservacaoIsotank
{
    public int Id { get; set; }
    public string CodigoIsotank { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.Now;
    public string AutorNome { get; set; } = string.Empty;
}
