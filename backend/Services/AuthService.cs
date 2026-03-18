using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;

namespace Backend.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;

    public AuthService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<(bool Ok, string? ErrorMessage, AuthResponse? Data)> RegisterAsync(RegisterRequest request)
    {
        // Validações simples (para iniciantes).
        if (string.IsNullOrWhiteSpace(request.Nome) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Senha))
        {
            return (false, "Dados inválidos", null);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        try
        {
            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing is not null)
            {
                return (false, "Usuário já existe", null);
            }
        }
        catch (MongoException)
        {
            return (false, "Erro ao conectar no banco", null);
        }

        // BCrypt gera um hash seguro da senha.
        // Na hora do login, usamos BCrypt.Verify para comparar.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

        var user = new User
        {
            Nome = request.Nome.Trim(),
            Email = email,
            PasswordHash = passwordHash
        };

        try
        {
            await _userRepository.CreateAsync(user);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Caso raro: dois cadastros ao mesmo tempo com o mesmo email.
            return (false, "Usuário já existe", null);
        }
        catch (MongoException)
        {
            return (false, "Erro ao conectar no banco", null);
        }

        return (true, null, new AuthResponse
        {
            Id = user.Id ?? string.Empty,
            Nome = user.Nome,
            Email = user.Email
        });
    }

    public async Task<(bool Ok, string? ErrorMessage, AuthResponse? Data)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Senha))
        {
            return (false, "Credenciais inválidas", null);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        User? user;
        try
        {
            user = await _userRepository.GetByEmailAsync(email);
        }
        catch (MongoException)
        {
            return (false, "Erro ao conectar no banco", null);
        }

        if (user is null)
        {
            return (false, "Credenciais inválidas", null);
        }

        var ok = BCrypt.Net.BCrypt.Verify(request.Senha, user.PasswordHash);
        if (!ok)
        {
            return (false, "Credenciais inválidas", null);
        }

        return (true, null, new AuthResponse
        {
            Id = user.Id ?? string.Empty,
            Nome = user.Nome,
            Email = user.Email
        });
    }
}

