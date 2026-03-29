using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

/// <summary>
/// Registro de certificado emitido (dados mínimos para exibir ou gerar documento depois).
/// </summary>
public class Certificado
{
    private string? _id;
    private string _participanteId = string.Empty;
    private string _eventoId = string.Empty;
    private string _nomeEvento = string.Empty;

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id
    {
        get => _id;
        set => _id = value;
    }

    public string ParticipanteId
    {
        get => _participanteId;
        set => _participanteId = value ?? string.Empty;
    }

    public string EventoId
    {
        get => _eventoId;
        set => _eventoId = value ?? string.Empty;
    }

    /// <summary>Nome do evento no momento da emissão (cópia para histórico se o evento for renomeado).</summary>
    public string NomeEvento
    {
        get => _nomeEvento;
        set => _nomeEvento = value ?? string.Empty;
    }

    public Certificado()
    {
    }

    public Certificado(string participanteId, string eventoId, string nomeEvento)
    {
        ParticipanteId = participanteId;
        EventoId = eventoId;
        NomeEvento = nomeEvento;
    }
}
