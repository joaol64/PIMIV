namespace Backend.DTOs;

/// <summary>Cria atividade vinculada a um evento (somente admin).</summary>
public class CreateAtividadeRequest
{
    public string UsuarioId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public DateTime Data { get; set; }

    public string EventoId { get; set; } = string.Empty;
}
