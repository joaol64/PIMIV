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

    /// <summary>Início do período em que o evento está ativo (gravar em UTC).</summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DataInicio { get; set; }

    /// <summary>Término do período em que o evento está ativo (gravar em UTC).</summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DataFim { get; set; }

    /// <summary>Campo legado em documentos antigos (única data de referência).</summary>
    [BsonIgnoreIfNull]
    public DateTime? Data { get; set; }

    /// <summary>Início efetivo para ordenação, validação e certificados.</summary>
    [BsonIgnore]
    public DateTime DataInicioEfetiva =>
        DataInicio != default ? DataInicio : Data ?? default;

    /// <summary>Término efetivo; se só existir <see cref="Data"/>, usa o mesmo instante.</summary>
    [BsonIgnore]
    public DateTime DataFimEfetiva =>
        DataFim != default ? DataFim : Data ?? DataInicioEfetiva;

    public Evento()
    {
    }

    public Evento(string nome, DateTime dataInicio, DateTime dataFim)
    {
        Nome = nome;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }
}
