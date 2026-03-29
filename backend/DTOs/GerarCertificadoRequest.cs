namespace Backend.DTOs;

/// <summary>Corpo do POST para gerar e registrar um certificado.</summary>
public class GerarCertificadoRequest
{
    public string ParticipanteId { get; set; } = string.Empty;

    public string EventoId { get; set; } = string.Empty;

    /// <summary>Se true, retorna HTML; se false, texto puro.</summary>
    public bool ComoHtml { get; set; } = true;
}
