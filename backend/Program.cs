using Backend.Config;
using Backend.Data;
using Backend.Serialization;
using Backend.Repositories;
using Backend.Services;
using DotNetEnv;

// Carrega backend/.env no processo (DATABASE_URL, MongoDbSettings__*, etc.) antes do host ler variáveis.
foreach (var envPath in new[]
         {
             Path.Combine(Directory.GetCurrentDirectory(), ".env"),
             Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env")),
         })
{
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
        break;
    }
}

var builder = WebApplication.CreateBuilder(args);

// Carrega configurações em ordem:
// 1) appsettings.json (base)
// 2) appsettings.{Ambiente}.json (sobrescreve no ambiente atual)
// 3) variáveis de ambiente (maior prioridade)
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Controllers = endpoints "tradicionais" (ex: AuthController com /api/auth/register e /api/auth/login)
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    opts.JsonSerializerOptions.Converters.Add(new JsonUtcDateTimeConverter());
    opts.JsonSerializerOptions.Converters.Add(new JsonNullableUtcDateTimeConverter());
});

// Swagger ajuda a testar a API no navegador; útil para inspecionar rotas e formatos de request/response.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================
// CORS (obrigatório)
// =========================
// O frontend (HTML/JS) roda separado do backend e chama a API via `fetch()`.
// Sem CORS, o navegador bloquearia as requisições por política de segurança.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        // Para simplificar integração: libera origem/método/header vindos do frontend.
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// =========================
// MongoDB via Environment Variables
// =========================
// IMPORTANTE:
// - NÃO colocamos connection string fixa no código.
// - Em produção (Render) e em dev você configura variáveis de ambiente.
//
// Exemplo no Windows (PowerShell):
//   $env:MongoDbSettings__ConnectionString = "mongodb://localhost:27017"
//   $env:MongoDbSettings__DatabaseName = "pim"
//
// O formato "MongoDbSettings__ConnectionString" (com __) significa:
// - Se você tem a classe MongoDbSettings { ConnectionString }, o .NET mapeia automaticamente.
// DATABASE_URL (comum em Render/Heroku) preenche ConnectionString quando appsettings está vazio.
builder.Services.Configure<MongoDbSettings>(opts =>
{
    builder.Configuration.GetSection(MongoDbSettings.SectionName).Bind(opts);
    if (string.IsNullOrWhiteSpace(opts.ConnectionString))
    {
        var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL")?.Trim();
        if (!string.IsNullOrWhiteSpace(dbUrl))
            opts.ConnectionString = dbUrl;
    }
});

// MongoDB compartilhado + repositórios e serviços (camadas)
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<EventoRepository>();
builder.Services.AddSingleton<AtividadeRepository>();
builder.Services.AddSingleton<ParticipanteRepository>();
builder.Services.AddSingleton<InscricaoRepository>();
builder.Services.AddSingleton<CertificadoRepository>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<EventoService>();
builder.Services.AddSingleton<AtividadeService>();
builder.Services.AddSingleton<InscricaoService>();
builder.Services.AddSingleton<CertificadoService>();
// Cria conta admin na subida, se AdminSeed estiver configurado
// (via appsettings.json, appsettings.{Ambiente}.json ou variáveis de ambiente).
builder.Services.AddHostedService<AdminSeedHostedService>();

var app = builder.Build();

// Porta dinâmica (Render usa PORT).
// Em desenvolvimento, se PORT não existir, usamos 5000.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

// Faz o servidor escutar na porta escolhida, em todas as interfaces de rede.
app.Urls.Add($"http://*:{port}");

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

// Aplica a policy CORS definida acima, para permitir chamadas do frontend via navegador.
// A API usa HTTP (não HTTPS) por padrão; assim o front pode chamar diretamente em ambientes locais.
// Se você quiser HTTPS depois, dá para ativar novamente com certificados.
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
