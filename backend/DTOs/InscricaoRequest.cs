namespace Backend.DTOs;

/// <summary>Inscrição do usuário comum em uma atividade.</summary>
public class InscricaoRequest
{
    /// <summary>Id da conta retornado no login (coleção Users no MongoDB).</summary>
    public string UsuarioId { get; set; } = string.Empty;

    public string AtividadeId { get; set; } = string.Empty;
}
