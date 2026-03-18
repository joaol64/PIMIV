namespace Backend.DTOs;

public class RegisterRequest
{
    // Nome exibido/associado ao usuário.
    public string Nome { get; set; } = string.Empty;

    // Email do usuário (normalizado no backend antes de persistir/buscar).
    public string Email { get; set; } = string.Empty;

    // Senha informada pelo cliente (o backend armazena apenas o hash).
    public string Senha { get; set; } = string.Empty;
}

