namespace SimuladorMBM.Models;

/// <summary>
/// Isotanque/container no MBM — registro completo com FK para Cliente e Produto.
/// O portal consome uma visão "achatada" (codigo, produto, cliente, status, previsão).
/// </summary>
public class Isotanque
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Etapa/status dentro da Depotce (Ag. Limpeza, Ag. Inspeção, etc.).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Data em que o isotanque entrou no status atual (para calcular "Dias no Status" como no BI).</summary>
    public DateTime? DataInicioStatus { get; set; }

    public DateTime? PrevisaoLiberacao { get; set; }

    /// <summary>Data de entrada no pátio (simulação MBM).</summary>
    public DateTime? DataEntrada { get; set; }

    /// <summary>Última atualização do status no MBM.</summary>
    public DateTime? UltimaAtualizacao { get; set; }

    /// <summary>Data/hora em que foi descarregado no pátio.</summary>
    public DateTime? DataHoraDescarregadoPatio { get; set; }

    /// <summary>Data/hora em que foi carregado no veículo.</summary>
    public DateTime? DataHoraCarregadoVeiculo { get; set; }

    /// <summary>Previsão de chegada no terminal portuário.</summary>
    public DateTime? PrevisaoChegadaTerminal { get; set; }

    /// <summary>URL da foto do isotanque (quando disponível no MBM).</summary>
    public string? UrlFoto { get; set; }

    /// <summary>Placa do veículo em que o isotanque foi carregado.</summary>
    public string? PlacaVeiculo { get; set; }

    public bool Ativo { get; set; } = true;
}