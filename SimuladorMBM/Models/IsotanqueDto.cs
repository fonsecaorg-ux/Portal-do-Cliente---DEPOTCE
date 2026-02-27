namespace SimuladorMBM.Models;

public class IsotanqueDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? DiasNoStatus { get; set; }
    public DateTime? PrevisaoLiberacao { get; set; }

    // ── Logística ──────────────────────────────────────
    public DateTime? DataHoraDescarregadoPatio { get; set; }
    public DateTime? DataHoraCarregadoVeiculo { get; set; }
    public DateTime? DataSaida { get; set; }
    public DateTime? PrevisaoChegadaTerminal { get; set; }
    public string? LocalizacaoPatio { get; set; }

    // ── Vistoria ───────────────────────────────────────
    public DateTime? DataVistoria { get; set; }
    public string? NomeVistoriador { get; set; }
    public string? NumeroLaudo { get; set; }
    public string? NumeroLacre { get; set; }

    // ── Dados técnicos ─────────────────────────────────
    public string? UltimoCarregamento { get; set; }
    public decimal? Tara { get; set; }
    public decimal? Capacidade { get; set; }
    public DateTime? ValidadeCertificado { get; set; }

    // ── Fotos ──────────────────────────────────────────
    public string? UrlFoto { get; set; }
    public string? UrlFoto2 { get; set; }
    public string? UrlFoto3 { get; set; }

    // ── Documentos ─────────────────────────────────────
    public string? UrlLaudoVistoria { get; set; }
    public string? UrlCertificadoLavagem { get; set; }

    /// <summary>Número do booking vinculado a este isotanque (quando houver).</summary>
    public string? NumeroBooking { get; set; }
}