namespace Backend.DTOs;

/// <summary>POST /api/eventos — datas como string ISO (ex.: retorno de Date.toISOString() no JS).</summary>
public class CreateEventoRequest
{
    public string UsuarioId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    /// <summary>ISO-8601, ex. 2026-04-08T12:00:00.000Z</summary>
    public string DataInicio { get; set; } = string.Empty;

    /// <summary>ISO-8601, ex. 2026-04-10T18:00:00.000Z</summary>
    public string DataFim { get; set; } = string.Empty;
}
