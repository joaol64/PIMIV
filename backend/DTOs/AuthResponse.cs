namespace Backend.DTOs;

public class AuthResponse
{
    // Identificador do usuário (Id do MongoDB).
    public string Id { get; set; } = string.Empty;

    // Nome do usuário.
    public string Nome { get; set; } = string.Empty;

    // Email do usuário.
    public string Email { get; set; } = string.Empty;

    // Status da conta para indicar se o vídeo já foi visualizado.
    public bool JaViuVideo { get; set; }
}

