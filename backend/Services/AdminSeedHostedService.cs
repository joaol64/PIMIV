using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Na subida da API, cria um usuário administrador se ainda não existir (configuração AdminSeed).
/// Troque email/senha em produção via variáveis de ambiente ou secrets.
/// </summary>
public class AdminSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeedHostedService> _logger;

    public AdminSeedHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AdminSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        // Lê appsettings (AdminSeed) ou variáveis AdminSeed__Email / AdminSeed__Password.
        var email = _configuration["AdminSeed:Email"]?.Trim();
        var password = _configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation(
                "AdminSeed não configurado (email/senha vazios). Nenhuma conta admin automática será criada.");
            return;
        }

        var emailNorm = email.ToLowerInvariant();

        try
        {
            var existente = await userRepository.GetByEmailAsync(emailNorm);
            if (existente is not null)
            {
                if (existente.TipoUsuario != TipoUsuario.Administrador)
                {
                    _logger.LogWarning(
                        "Já existe usuário com email {Email}, mas não é Administrador. Ajuste o campo no MongoDB se precisar.",
                        emailNorm);
                }

                return;
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var admin = new User
            {
                Nome = "Administrador",
                Email = emailNorm,
                PasswordHash = hash,
                TipoUsuario = TipoUsuario.Administrador,
                JaViuVideo = false
            };

            await userRepository.CreateAsync(admin);
            _logger.LogInformation(
                "Conta administrador criada para {Email}. Altere a senha padrão em ambientes reais.",
                emailNorm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Não foi possível criar a conta administrador automática.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
