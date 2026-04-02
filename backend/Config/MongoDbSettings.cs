namespace Backend.Config;

public class MongoDbSettings
{
    // Nome da seção usada pelo Configuration/Environment Variables.
    // Exemplo: MongoDbSettings__ConnectionString
    public const string SectionName = "MongoDbSettings";

    // NUNCA fixe isso no código. Configure via:
    // - MongoDbSettings__ConnectionString, ou
    // - DATABASE_URL (comum em hospedagem; o Program.cs copia para ConnectionString se estiver vazio), ou
    // - backend/.env com DATABASE_URL=...
    public string ConnectionString { get; set; } = string.Empty;

    // Nome do banco dentro do MongoDB (ex: "pim")
    // Configure via variável de ambiente:
    // MongoDbSettings__DatabaseName = "pim"
    public string DatabaseName { get; set; } = "pim";
}

