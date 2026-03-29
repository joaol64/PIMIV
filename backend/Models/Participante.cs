using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

/// <summary>
/// Pessoa inscrita nos eventos (nome e email vêm da base <see cref="Usuario"/>).
/// Pode ser persistido em collection própria (ex.: "Participantes"), separado de <see cref="User"/> (login).
/// </summary>
public class Participante : Usuario
{
    private string? _id;

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>Necessário para o MongoDB materializar o documento.</summary>
    public Participante()
    {
    }

    /// <summary>Cria participante com nome e email (validação básica nas propriedades da classe base).</summary>
    public Participante(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }
}
