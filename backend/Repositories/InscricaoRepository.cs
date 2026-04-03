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

    public async Task<long> CountByAtividadeAsync(string atividadeId)
    {
        return await _inscricoes.CountDocumentsAsync(i => i.AtividadeId == atividadeId);
    }

    /// <summary>Total de inscrições cujo <see cref="Inscricao.AtividadeId"/> está na lista.</summary>
    public async Task<long> CountByAtividadesAsync(IReadOnlyCollection<string> atividadeIds)
    {
        if (atividadeIds == null || atividadeIds.Count == 0)
        {
            return 0;
        }

        var ids = atividadeIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (ids.Count == 0)
        {
            return 0;
        }

        var filter = Builders<Inscricao>.Filter.In(i => i.AtividadeId, ids);
        return await _inscricoes.CountDocumentsAsync(filter);
    }

    /// <summary>Quantidade de participantes distintos com inscrição em qualquer atividade da lista.</summary>
    public async Task<long> CountParticipantesDistintosPorAtividadesAsync(IReadOnlyCollection<string> atividadeIds)
    {
        if (atividadeIds == null || atividadeIds.Count == 0)
        {
            return 0;
        }

        var ids = atividadeIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (ids.Count == 0)
        {
            return 0;
        }

        var filter = Builders<Inscricao>.Filter.In(i => i.AtividadeId, ids);
        var list = await _inscricoes.Find(filter).ToListAsync();
        return list.Select(i => i.ParticipanteId).Distinct().LongCount();
    }

    public async Task<List<Inscricao>> ListarTodosAsync()
    {
        return await _inscricoes.Find(_ => true).ToListAsync();
    }
}
