using Backend.Config;
using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Repositories;

public class UserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IOptions<MongoDbSettings> mongoOptions)
    {
        var settings = mongoOptions.Value;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            // Explicação didática:
            // Se você não configurar a variável de ambiente MongoDbSettings__ConnectionString,
            // o backend não sabe como conectar no MongoDB.
            throw new InvalidOperationException(
                "MongoDbSettings__ConnectionString não foi configurada nas variáveis de ambiente."
            );
        }

        // Por padrão, o driver pode demorar ~30s tentando conectar se o MongoDB estiver desligado.
        // Para ficar mais "amigável" para iniciantes, reduzimos o timeout de seleção de servidor.
        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(3);
        var client = new MongoClient(clientSettings);
        var database = client.GetDatabase(settings.DatabaseName);

        // Collection pedida no enunciado: Users
        _users = database.GetCollection<User>("Users");

        // Boa prática: garantir que Email seja único.
        // Isso evita duplicar usuários com o mesmo email.
        //
        // Observação:
        // Se o MongoDB estiver desligado, a criação do índice pode falhar.
        // Não vale "derrubar" a aplicação por isso — o importante é o Mongo estar ligado quando você usar a API.
        try
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));
        }
        catch (MongoException)
        {
            // Ignoramos aqui; operações (Find/Insert) vão falhar com erro claro se não houver conexão.
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }
}

