using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

/// <summary>
/// Evento acadêmico (ex.: semana científica, congresso).
/// Persistido em uma collection MongoDB dedicada (ex.: "Eventos").
/// </summary>
public class Evento
{
    private string? _id;
    private string _nome = string.Empty;
    private DateTime _data;

    /// <summary>Identificador gerado pelo MongoDB.</summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>Nome do evento.</summary>
    public string Nome
    {
        get => _nome;
        set => _nome = value ?? string.Empty;
    }

    /// <summary>Data de referência do evento (armazenada como o driver serializar; em geral use UTC em produção).</summary>
    public DateTime Data
    {
        get => _data;
        set => _data = value;
    }

    /// <summary>Construtor vazio: necessário para o driver MongoDB desserializar documentos.</summary>
    public Evento()
    {
    }

    /// <summary>Construtor conveniente para criar evento em memória antes de inserir no banco.</summary>
    public Evento(string nome, DateTime data)
    {
        Nome = nome;
        Data = data;
    }
}
