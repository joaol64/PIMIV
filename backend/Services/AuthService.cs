using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;

namespace Backend.Services;

public class AuthService
{
    private const int MaxNomeEmailLength = 30;
    private const int MinPasswordLength = 6;

    private readonly UserRepository _userRepository;

    public AuthService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // Retorna uma tupla para padronizar respostas internas:
    // Ok = sucesso, ErrorMessage = motivo em caso de falha e Data = usuário (sem senha) quando Ok.
    public async Task<(bool Ok, string? ErrorMessage, AuthResponse? Data)> RegisterAsync(RegisterRequest request)
    {
        // Validações mínimas para garantir que os dados obrigatórios existam.
        if (string.IsNullOrWhiteSpace(request.Nome) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Senha))
        {
            return (false, "Dados inválidos", null);
        }

        var nomeTrim = request.Nome.Trim();
        if (nomeTrim.Length > MaxNomeEmailLength)
        {
            return (false, $"Nome deve ter no máximo {MaxNomeEmailLength} caracteres", null);
        }

        var emailTrimmed = request.Email.Trim();
        if (emailTrimmed.Length > MaxNomeEmailLength)
        {
            return (false, $"Email deve ter no máximo {MaxNomeEmailLength} caracteres", null);
        }

        if (request.Senha.Length < MinPasswordLength)
        {
            return (false, $"A senha deve ter no mínimo {MinPasswordLength} caracteres", null);
        }

        // Normalizamos o email para manter comparações consistentes (sem diferenciar maiúsculas/minúsculas).
        var email = emailTrimmed.ToLowerInvariant();

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
        // No login, usamos BCrypt.Verify para comparar a senha informada com o hash armazenado.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

        var user = new User
        {
            Nome = nomeTrim,
            Email = email,
            PasswordHash = passwordHash
        };

        try
        {
            await _userRepository.CreateAsync(user);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Caso raro: concorrência ao inserir (mesmo email) em função da constraint de unicidade.
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
            Email = user.Email,
            JaViuVideo = user.JaViuVideo
        });
    }

    // Fluxo de login:
    // 1) valida inputs, 2) busca usuário por email, 3) valida hash da senha com BCrypt, 4) retorna AuthResponse.
    public async Task<(bool Ok, string? ErrorMessage, AuthResponse? Data)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Senha))
        {
            return (false, "Credenciais inválidas", null);
        }

        var emailTrimmed = request.Email.Trim();
        if (emailTrimmed.Length > MaxNomeEmailLength || request.Senha.Length < MinPasswordLength)
        {
            return (false, "Credenciais inválidas", null);
        }

        // Normalizamos o email para buscar o registro de forma consistente.
        var email = emailTrimmed.ToLowerInvariant();

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

        // Verifica se a senha informada corresponde ao hash armazenado.
        var ok = BCrypt.Net.BCrypt.Verify(request.Senha, user.PasswordHash);
        if (!ok)
        {
            return (false, "Credenciais inválidas", null);
        }

        return (true, null, new AuthResponse
        {
            Id = user.Id ?? string.Empty,
            Nome = user.Nome,
            Email = user.Email,
            JaViuVideo = user.JaViuVideo
        });
    }

    public async Task<(bool Ok, string? ErrorMessage, AuthResponse? Data)> MarkVideoAsSeenAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return (false, "Usuário inválido", null);
        }

        try
        {
            var updatedUser = await _userRepository.MarkVideoAsSeenAsync(userId.Trim());
            if (updatedUser is null)
            {
                return (false, "Usuário não encontrado", null);
            }

            return (true, null, new AuthResponse
            {
                Id = updatedUser.Id ?? string.Empty,
                Nome = updatedUser.Nome,
                Email = updatedUser.Email,
                JaViuVideo = updatedUser.JaViuVideo
            });
        }
        catch (MongoException)
        {
            return (false, "Erro ao conectar no banco", null);
        }
    }
}

