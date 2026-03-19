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
            // Se a connection string não for fornecida via configuração/variável de ambiente,
            // o backend não consegue estabelecer conexão com o MongoDB.
            throw new InvalidOperationException(
                "MongoDbSettings__ConnectionString não foi configurada nas variáveis de ambiente."
            );
        }

        // Ajuste para evitar esperas longas: se o MongoDB estiver indisponível,
        // reduzimos o tempo de seleção de servidor antes de falhar com erro.
        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
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

    // Busca um usuário pelo email (retorna `null` quando não existir).
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }

    // Insere um novo usuário na collection.
    // A unicidade do email é garantida pelo índice único criado no construtor.
    public async Task CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }

    public async Task<User?> MarkVideoAsSeenAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Set(u => u.JaViuVideo, true);
        var options = new FindOneAndUpdateOptions<User>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _users.FindOneAndUpdateAsync(filter, update, options);
    }
}

