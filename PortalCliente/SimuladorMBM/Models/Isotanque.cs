namespace SimuladorMBM.Models;

/// <summary>
/// Isotanque/container no MBM — registro completo com FK para Cliente e Produto.
/// O portal consome uma visão “achatada” (codigo, produto, cliente, status, previsão).
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

    /// <summary>Data de saída do isotanque do pátio (gate-out).</summary>
    public DateTime? DataSaida { get; set; }

    /// <summary>Previsão de chegada no terminal portuário.</summary>
    public DateTime? PrevisaoChegadaTerminal { get; set; }

    /// <summary>Data/hora descarregado no terminal (gate-in).</summary>
    public DateTime? DataHoraDescarregadoTerminal { get; set; }

    /// <summary>Tipo do isotanque (ex: T11, T14).</summary>
    public string? Tipo { get; set; }

    /// <summary>Proprietário / armador.</summary>
    public string? ProprietarioArmador { get; set; }

    /// <summary>Placa do veículo em que foi carregado.</summary>
    public string? PlacaVeiculo { get; set; }

    /// <summary>Data limite do SLA para a etapa atual.</summary>
    public DateTime? SlaLimite { get; set; }

    /// <summary>Data de vencimento do teste periódico (2,5 ou 5 anos).</summary>
    public DateTime? TestePeriodicoVencimento { get; set; }

    /// <summary>Valor acumulado de reparos (R$).</summary>
    public decimal? ReparoAcumuladoValor { get; set; }

    /// <summary>URLs de fotos da vistoria (separadas por vírgula).</summary>
    public string? UrlsFotos { get; set; }

    /// <summary>Localização: pátio.</summary>
    public string? Patio { get; set; }

    /// <summary>Localização: bloco.</summary>
    public string? Bloco { get; set; }

    /// <summary>Localização: fila.</summary>
    public string? Fila { get; set; }

    /// <summary>Localização: pilha.</summary>
    public string? Pilha { get; set; }

    /// <summary>URL da foto do isotanque (quando disponível no MBM).</summary>
    public string? UrlFoto { get; set; }

    /// <summary>URL do laudo de vistoria (EIR) — path relativo ou absoluto.</summary>
    public string? UrlLaudoVistoria { get; set; }

    /// <summary>URL do certificado de lavagem.</summary>
    public string? UrlCertificadoLavagem { get; set; }

    /// <summary>Número do booking vinculado a este isotanque (quando houver).</summary>
    public string? NumeroBooking { get; set; }

    public bool Ativo { get; set; } = true;
}
