namespace Backend.DTOs;

public class LoginRequest
{
    // Email do usuário a autenticar.
    public string Email { get; set; } = string.Empty;

    // Senha informada pelo cliente (será comparada via BCrypt no backend).
    public string Senha { get; set; } = string.Empty;
}

