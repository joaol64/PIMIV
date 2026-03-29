namespace Backend.Models;

/// <summary>
/// Classe base abstrata para representar qualquer pessoa/usuário no domínio.
/// Encapsula nome e email com campos privados e propriedades públicas que aplicam validação básica.
/// Não é instanciada diretamente: use <see cref="User"/> (conta de login), <see cref="Participante"/> (eventos), etc.
/// </summary>
public abstract class Usuario
{
    /// <summary>Limite máximo de caracteres do nome (alinha com a API de cadastro).</summary>
    public const int MaxNomeLength = 30;

    /// <summary>Limite máximo de caracteres do email (alinha com a API de cadastro).</summary>
    public const int MaxEmailLength = 30;

    // Campos privados: o estado interno não é exposto diretamente; o acesso passa pelas propriedades.
    private string _nome = string.Empty;
    private string _email = string.Empty;

    /// <summary>Nome completo ou apelido do usuário (obrigatório, com tamanho máximo).</summary>
    public string Nome
    {
        get => _nome;
        set
        {
            // Validação básica: não permite vazio nem só espaços.
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Nome não pode ser vazio.", nameof(value));
            }

            var trimmed = value.Trim();
            if (trimmed.Length > MaxNomeLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Nome deve ter no máximo {MaxNomeLength} caracteres.");
            }

            _nome = trimmed;
        }
    }

    /// <summary>Email usado como identificador de login (obrigatório, normalizado em minúsculas).</summary>
    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Email não pode ser vazio.", nameof(value));
            }

            var trimmed = value.Trim();
            if (trimmed.Length > MaxEmailLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Email deve ter no máximo {MaxEmailLength} caracteres.");
            }

            // Minúsculas evitam duplicidade por diferença de maiúsculas/minúsculas.
            _email = trimmed.ToLowerInvariant();
        }
    }
}
