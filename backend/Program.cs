using Backend.Config;
using Backend.Repositories;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers = endpoints "tradicionais" (ex: AuthController com /api/auth/register e /api/auth/login)
builder.Services.AddControllers();

// Swagger ajuda a testar a API no navegador (bom para iniciantes).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================
// CORS (obrigatório)
// =========================
// O frontend (HTML/JS) vai rodar separado do backend e chamará a API via fetch().
// Sem CORS, o navegador bloquearia as requisições por segurança.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
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
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(MongoDbSettings.SectionName)
);

// Repositórios e serviços (camadas)
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<AuthService>();

var app = builder.Build();

// Porta dinâmica (Render usa PORT).
// Em desenvolvimento, se PORT não existir, usamos 5000.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

// A API deste projeto é HTTP (não HTTPS) para simplificar para iniciantes e facilitar o fetch local.
// Se você quiser HTTPS depois, dá para ativar novamente com certificados.
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
