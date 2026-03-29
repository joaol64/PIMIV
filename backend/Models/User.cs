using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

/// <summary>
/// Entidade persistida na collection "Users" do MongoDB.
/// Herda <see cref="Usuario"/> (nome/email encapsulados na base) e acrescenta autenticação e tipo de perfil.
/// <see cref="TipoUsuario"/> distingue Participante de Administrador sem precisar de collections separadas.
/// </summary>
public class User : Usuario
{
    // Identificador gerado pelo MongoDB (ObjectId como string na API).
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    private string _passwordHash = string.Empty;
    private bool _jaViuVideo;
    private TipoUsuario _tipoUsuario = TipoUsuario.Participante;

    /// <summary>Hash BCrypt da senha — nunca armazenar senha em texto puro.</summary>
    public string PasswordHash
    {
        get => _passwordHash;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Hash de senha não pode ser vazio.", nameof(value));
            }

            _passwordHash = value;
        }
    }

    /// <summary>Indica se o usuário já marcou o vídeo em Libras como visto.</summary>
    public bool JaViuVideo
    {
        get => _jaViuVideo;
        set => _jaViuVideo = value;
    }

    /// <summary>
    /// Participante ou Administrador. Cadastros via API pública usam <see cref="TipoUsuario.Participante"/>.
    /// Administradores podem ser promovidos manualmente no banco ou por fluxo interno futuro.
    /// </summary>
    public TipoUsuario TipoUsuario
    {
        get => _tipoUsuario;
        set => _tipoUsuario = value;
    }
}
