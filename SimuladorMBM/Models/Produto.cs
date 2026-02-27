namespace SimuladorMBM.Models;

/// <summary>
/// Produto químico cadastrado no MBM.
/// </summary>
public class Produto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? ClasseRisco { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Isotanque> Isotanques { get; set; } = new List<Isotanque>();
}
