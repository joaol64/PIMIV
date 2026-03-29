namespace Backend.DTOs;

/// <summary>Certificado salvo no MongoDB + conteúdo gerado (HTML ou texto).</summary>
public class CertificadoGeradoResponse
{
    public string? Id { get; set; }

    public string ParticipanteId { get; set; } = string.Empty;

    public string EventoId { get; set; } = string.Empty;

    public string NomeEvento { get; set; } = string.Empty;

    public string Conteudo { get; set; } = string.Empty;
}
