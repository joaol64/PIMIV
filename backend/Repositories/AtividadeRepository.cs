using Backend.Data;
using Backend.Models;
using MongoDB.Driver;

namespace Backend.Repositories;

/// <summary>Acesso à collection "Atividades".</summary>
public class AtividadeRepository
{
    private readonly IMongoCollection<Atividade> _atividades;

    public AtividadeRepository(MongoDbContext ctx)
    {
        _atividades = ctx.Database.GetCollection<Atividade>("Atividades");
    }

    public async Task CreateAsync(Atividade atividade)
    {
        await _atividades.InsertOneAsync(atividade);
    }

    public async Task<Atividade?> GetByIdAsync(string id)
    {
        return await _atividades.Find(a => a.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>Carrega atividades para filtrar/ordenar no serviço com LINQ.</summary>
    public async Task<List<Atividade>> ListarTodasAsync()
    {
        return await _atividades.Find(_ => true).ToListAsync();
    }
}
