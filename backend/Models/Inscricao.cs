using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

/// <summary>
/// Liga um participante a uma atividade (inscrição).
/// </summary>
public class Inscricao
{
    private string? _id;
    private string _participanteId = string.Empty;
    private string _atividadeId = string.Empty;

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>Id (ObjectId string) do <see cref="Participante"/>.</summary>
    public string ParticipanteId
    {
        get => _participanteId;
        set => _participanteId = value ?? string.Empty;
    }

    /// <summary>Id (ObjectId string) da <see cref="Atividade"/>.</summary>
    public string AtividadeId
    {
        get => _atividadeId;
        set => _atividadeId = value ?? string.Empty;
    }

    public Inscricao()
    {
    }

    public Inscricao(string participanteId, string atividadeId)
    {
        ParticipanteId = participanteId;
        AtividadeId = atividadeId;
    }
}
