using Backend.Data;
using Backend.Models;
using MongoDB.Driver;

namespace Backend.Repositories;

public class UserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext ctx)
    {
        _users = ctx.Database.GetCollection<User>("Users");

        try
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));
        }
        catch (MongoException)
        {
            // Índice pode falhar se o Mongo estiver offline; operações seguintes indicarão o erro.
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }

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
