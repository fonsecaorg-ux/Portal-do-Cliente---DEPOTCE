namespace PortalCliente.Models;

/// <summary>
/// Isotanque/container — dados de consulta originados do MBM (sistema base).
/// O portal apenas exibe; não há cadastro/edição pelo cliente.
/// </summary>
public class Container
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    /// <summary>Etapa/status do isotanque dentro da Depotce (ex: Ag. Limpeza, Ag. Inspeção).</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Dias no status atual (como no relatório "Isotank Vazio - Dias no Status" do BI).</summary>
    public int? DiasNoStatus { get; set; }
    /// <summary>Previsão de liberação e demais informações correlatas.</summary>
    public DateTime? PrevisaoLiberacao { get; set; }
    /// <summary>Cliente dono do isotanque. Usado para filtrar quando houver login.</summary>
    public string Cliente { get; set; } = string.Empty;
    /// <summary>Data/hora descarregado no pátio (quando informado no MBM).</summary>
    public DateTime? DataHoraDescarregadoPatio { get; set; }
    /// <summary>Data/hora carregado no veículo (quando informado no MBM).</summary>
    public DateTime? DataHoraCarregadoVeiculo { get; set; }
    /// <summary>Data de saída do isotanque do pátio (gate-out).</summary>
    public DateTime? DataSaida { get; set; }
    /// <summary>Previsão de chegada no terminal portuário (quando informado no MBM).</summary>
    public DateTime? PrevisaoChegadaTerminal { get; set; }
    /// <summary>URL da foto do isotanque (quando disponível no MBM).</summary>
    public string? UrlFoto { get; set; }
    /// <summary>Placa do veículo em que o isotanque foi carregado (quando informado no MBM).</summary>
    public string? PlacaVeiculo { get; set; }
    // ─── Campos adicionais (MBM completo — espelho do IsotanqueDto) ─────
    /// <summary>Data de entrada no pátio (para "dias no pátio" total).</summary>
    public DateTime? DataEntrada { get; set; }
    /// <summary>Data em que entrou no status atual.</summary>
    public DateTime? DataInicioStatus { get; set; }
    /// <summary>Data/hora descarregado no terminal (gate-in).</summary>
    public DateTime? DataHoraDescarregadoTerminal { get; set; }
    /// <summary>Tipo do isotanque (ex: T11, T14).</summary>
    public string? Tipo { get; set; }
    /// <summary>Proprietário / armador.</summary>
    public string? ProprietarioArmador { get; set; }
    /// <summary>Data limite do SLA para a etapa atual.</summary>
    public DateTime? SlaLimite { get; set; }
    /// <summary>Data de vencimento do teste periódico (2,5 ou 5 anos).</summary>
    public DateTime? TestePeriodicoVencimento { get; set; }
    /// <summary>Valor acumulado de reparos (R$).</summary>
    public decimal? ReparoAcumuladoValor { get; set; }
    /// <summary>URLs de fotos da vistoria (separadas por vírgula para galeria).</summary>
    public string? UrlsFotos { get; set; }
    /// <summary>Localização: pátio, bloco, fila, pilha.</summary>
    public string? Patio { get; set; }
    public string? Bloco { get; set; }
    public string? Fila { get; set; }
    public string? Pilha { get; set; }
    /// <summary>URL do certificado de lavagem.</summary>
    public string? UrlCertificadoLavagem { get; set; }
    /// <summary>URL do laudo de vistoria / EIR (PDF).</summary>
    public string? UrlLaudoVistoria { get; set; }
    /// <summary>Número do booking vinculado a este isotanque (quando houver).</summary>
    public string? NumeroBooking { get; set; }
}