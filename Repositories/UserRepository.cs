using MongoDB.Driver;
using MyApiProject.Models;

namespace MyApiProject.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext ctx)
    {
        _users = ctx.Users;
    }

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User> CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        user.Id = id;
        var result = await _users.ReplaceOneAsync(u => u.Id == id, user);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
