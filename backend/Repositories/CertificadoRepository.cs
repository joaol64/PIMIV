using Backend.Data;
using Backend.Models;
using MongoDB.Driver;

namespace Backend.Repositories;

/// <summary>Collection "Certificados" no MongoDB.</summary>
public class CertificadoRepository
{
    private readonly IMongoCollection<Certificado> _certificados;

    public CertificadoRepository(MongoDbContext ctx)
    {
        _certificados = ctx.Database.GetCollection<Certificado>("Certificados");
    }

    public async Task CreateAsync(Certificado certificado)
    {
        await _certificados.InsertOneAsync(certificado);
    }

    public async Task<Certificado?> GetByIdAsync(string id)
    {
        return await _certificados.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Certificado>> ListarTodosAsync()
    {
        return await _certificados.Find(_ => true).ToListAsync();
    }
}
