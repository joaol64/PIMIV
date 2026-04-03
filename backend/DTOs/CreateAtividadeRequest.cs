namespace Backend.DTOs;

/// <summary>Cria atividade vinculada a um evento (somente admin).</summary>
public class CreateAtividadeRequest
{
    public string UsuarioId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    /// <summary>Início (ISO-8601). Se vazio, usa <see cref="Data"/> (legado).</summary>
    public string DataInicio { get; set; } = string.Empty;

    /// <summary>Término (ISO-8601). Se vazio, usa o mesmo instante do início.</summary>
    public string DataFim { get; set; } = string.Empty;

    /// <summary>Legado: único instante quando <see cref="DataInicio"/> não é enviado.</summary>
    public string? Data { get; set; }

    public string EventoId { get; set; } = string.Empty;
}
