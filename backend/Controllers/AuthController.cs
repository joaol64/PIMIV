using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Usuário já existe")
            {
                return Conflict(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Erro ao conectar no banco")
            {
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            return BadRequest(new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Erro ao conectar no banco")
            {
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            return Unauthorized(new ErrorResponse { Message = result.ErrorMessage ?? "Credenciais inválidas" });
        }

        return Ok(result.Data);
    }
}

