namespace Backend.DTOs;

/// <summary>Corpo do POST para criar evento. <see cref="UsuarioId"/> deve ser de um administrador.</summary>
public class CreateEventoRequest
{
    /// <summary>Id MongoDB do usuário admin (retornado no login).</summary>
    public string UsuarioId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public DateTime Data { get; set; }
}
