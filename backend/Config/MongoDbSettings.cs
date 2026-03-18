namespace Backend.Config;

public class MongoDbSettings
{
    // Nome da seção usada pelo Configuration/Environment Variables.
    // Exemplo: MongoDbSettings__ConnectionString
    public const string SectionName = "MongoDbSettings";

    // NUNCA fixe isso no código. Configure via variável de ambiente:
    // MongoDbSettings__ConnectionString = "mongodb://localhost:27017"
    public string ConnectionString { get; set; } = string.Empty;

    // Nome do banco dentro do MongoDB (ex: "pim")
    // Configure via variável de ambiente:
    // MongoDbSettings__DatabaseName = "pim"
    public string DatabaseName { get; set; } = "pim";
}

