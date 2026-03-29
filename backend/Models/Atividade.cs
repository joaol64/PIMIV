using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

/// <summary>
/// Atividade vinculada a um evento (ex.: palestra, minicurso).
/// O <see cref="EventoId"/> referencia o documento do evento pai.
/// </summary>
public class Atividade
{
    private string? _id;
    private string _nome = string.Empty;
    private DateTime _data;
    private string _eventoId = string.Empty;

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id
    {
        get => _id;
        set => _id = value;
    }

    public string Nome
    {
        get => _nome;
        set => _nome = value ?? string.Empty;
    }

    public DateTime Data
    {
        get => _data;
        set => _data = value;
    }

    /// <summary>Id (ObjectId string) do <see cref="Evento"/> ao qual esta atividade pertence.</summary>
    public string EventoId
    {
        get => _eventoId;
        set => _eventoId = value ?? string.Empty;
    }

    public Atividade()
    {
    }

    public Atividade(string nome, DateTime data, string eventoId)
    {
        Nome = nome;
        Data = data;
        EventoId = eventoId;
    }
}
