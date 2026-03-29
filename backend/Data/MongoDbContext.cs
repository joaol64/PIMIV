using Backend.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Backend.Data;

/// <summary>
/// Expõe o <see cref="IMongoDatabase"/> único da aplicação para todos os repositórios.
/// Evita repetir connection string e criação de cliente em cada classe.
/// </summary>
public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IOptions<MongoDbSettings> mongoOptions)
    {
        var settings = mongoOptions.Value;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "MongoDbSettings__ConnectionString não foi configurada nas variáveis de ambiente.");
        }

        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        var client = new MongoClient(clientSettings);
        Database = client.GetDatabase(settings.DatabaseName);
    }
}
