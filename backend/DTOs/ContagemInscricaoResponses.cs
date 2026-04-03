namespace Backend.DTOs;

/// <summary>Total de inscrições numa atividade (uma por participante, no máximo).</summary>
public class ContagemInscricaoAtividadeResponse
{
    public long Total { get; set; }
}

/// <summary>
/// Agregado do evento: <see cref="TotalInscricoes"/> soma todas as inscrições em qualquer atividade do evento;
/// <see cref="ParticipantesDistintos"/> conta quantos participantes diferentes têm ao menos uma inscrição.
/// </summary>
public class ContagemInscricaoEventoResponse
{
    public long TotalInscricoes { get; set; }

    public long ParticipantesDistintos { get; set; }
}
