namespace SimuladorMBM.Models;

/// <summary>
/// Cliente cadastrado no MBM (sistema base).
/// </summary>
public class Cliente
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? EmailContato { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Isotanque> Isotanques { get; set; } = new List<Isotanque>();
}
