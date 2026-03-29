using Backend.Data;
using Backend.Models;
using MongoDB.Driver;

namespace Backend.Repositories;

/// <summary>Acesso à collection "Inscricoes".</summary>
public class InscricaoRepository
{
    private readonly IMongoCollection<Inscricao> _inscricoes;

    public InscricaoRepository(MongoDbContext ctx)
    {
        _inscricoes = ctx.Database.GetCollection<Inscricao>("Inscricoes");
    }

    public async Task CreateAsync(Inscricao inscricao)
    {
        await _inscricoes.InsertOneAsync(inscricao);
    }

    public async Task<Inscricao?> GetByIdAsync(string id)
    {
        return await _inscricoes.Find(i => i.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>Verifica se já existe inscrição do participante na mesma atividade.</summary>
    public async Task<bool> ExisteAsync(string participanteId, string atividadeId)
    {
        var count = await _inscricoes.CountDocumentsAsync(
            i => i.ParticipanteId == participanteId && i.AtividadeId == atividadeId);
        return count > 0;
    }

    public async Task<List<Inscricao>> ListarTodosAsync()
    {
        return await _inscricoes.Find(_ => true).ToListAsync();
    }
}
