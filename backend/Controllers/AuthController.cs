using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Controlador responsável por rotas de autenticação:
    // - `POST /api/auth/register` para cadastro
    // - `POST /api/auth/login` para login
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/register
    // Retorna:
    // - 200 com `AuthResponse` quando o cadastro é bem-sucedido
    // - 409 quando o email já está cadastrado
    // - 500/400 quando o backend reporta erro de banco ou validação
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Usuário já existe")
            {
                // Mantemos a semântica de "conflito" para duplicidade (mesmo email).
                return Conflict(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Erro ao conectar no banco")
            {
                // Quando o backend não consegue acessar o MongoDB, retornamos erro 500.
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            // Qualquer outro erro de validação/entrada vira 400.
            return BadRequest(new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Data);
    }

    // POST /api/auth/login
    // Retorna:
    // - 200 com `AuthResponse` quando as credenciais são válidas
    // - 401 quando email/senha não correspondem
    // - 500 quando o backend reporta falha de conexão com o banco
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Erro ao conectar no banco")
            {
                // Erro de infraestrutura/banco deve aparecer como 500.
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            // Para credenciais inválidas, mantemos o 401 (não autorizado).
            return Unauthorized(new ErrorResponse { Message = result.ErrorMessage ?? "Credenciais inválidas" });
        }

        return Ok(result.Data);
    }
}

