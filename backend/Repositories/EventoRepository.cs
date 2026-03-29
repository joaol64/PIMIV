using Backend.Data;
using Backend.Models;
using MongoDB.Driver;

namespace Backend.Repositories;

/// <summary>Acesso à collection "Eventos" no MongoDB.</summary>
public class EventoRepository
{
    private readonly IMongoCollection<Evento> _eventos;

    public EventoRepository(MongoDbContext ctx)
    {
        _eventos = ctx.Database.GetCollection<Evento>("Eventos");
    }

    public async Task CreateAsync(Evento evento)
    {
        await _eventos.InsertOneAsync(evento);
    }

    public async Task<Evento?> GetByIdAsync(string id)
    {
        return await _eventos.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>Retorna todos os eventos (o serviço aplica ordenação com LINQ).</summary>
    public async Task<List<Evento>> ListarTodosAsync()
    {
        return await _eventos.Find(_ => true).ToListAsync();
    }
}
