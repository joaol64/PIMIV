using Backend.Data;
using Backend.Models;
using MongoDB.Driver;

namespace Backend.Repositories;

/// <summary>Acesso à collection "Participantes".</summary>
public class ParticipanteRepository
{
    private readonly IMongoCollection<Participante> _participantes;

    public ParticipanteRepository(MongoDbContext ctx)
    {
        _participantes = ctx.Database.GetCollection<Participante>("Participantes");
    }

    public async Task CreateAsync(Participante participante)
    {
        await _participantes.InsertOneAsync(participante);
    }

    public async Task<Participante?> GetByEmailAsync(string email)
    {
        var normalizado = email.Trim().ToLowerInvariant();
        return await _participantes.Find(p => p.Email == normalizado).FirstOrDefaultAsync();
    }

    public async Task<Participante?> GetByIdAsync(string id)
    {
        return await _participantes.Find(p => p.Id == id).FirstOrDefaultAsync();
    }
}
