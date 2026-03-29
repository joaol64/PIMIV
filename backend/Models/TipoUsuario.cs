namespace Backend.Models;

/// <summary>
/// Define o perfil da conta no sistema.
/// Participante: uso típico do app (ex.: conteúdo, vídeo).
/// Administrador: preparado para futuras rotas de gestão (eventos, usuários, etc.).
/// O valor é persistido no MongoDB junto com o documento do <see cref="User"/>.
/// </summary>
public enum TipoUsuario
{
    /// <summary>Usuário padrão criado pelo cadastro público.</summary>
    Participante = 0,

    /// <summary>Conta com permissões elevadas (definir via banco ou fluxo interno, não pelo register público).</summary>
    Administrador = 1
}
