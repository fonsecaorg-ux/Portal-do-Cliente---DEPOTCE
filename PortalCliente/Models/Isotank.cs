namespace PortalCliente.Models;

public class Isotank
{
    public string Codigo { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PrevisaoLiberacao { get; set; }
}