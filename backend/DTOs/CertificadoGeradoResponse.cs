namespace Backend.DTOs;

/// <summary>Certificado salvo no MongoDB + conteúdo gerado (HTML ou texto).</summary>
public class CertificadoGeradoResponse
{
    public string? Id { get; set; }

    public string ParticipanteId { get; set; } = string.Empty;

    public string EventoId { get; set; } = string.Empty;

    public string NomeEvento { get; set; } = string.Empty;

    public string NomeParticipante { get; set; } = string.Empty;

    /// <summary>Quantidade de eventos distintos com ao menos uma inscrição.</summary>
    public int TotalEventos { get; set; }

    /// <summary>Quantidade de atividades distintas em que há inscrição.</summary>
    public int TotalAtividades { get; set; }

    public string Conteudo { get; set; } = string.Empty;
}
