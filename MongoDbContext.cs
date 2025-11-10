using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MyApiProject.Models;

namespace MyApiProject;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }
    public string UsersCollectionName { get; }

    public MongoDbContext(IConfiguration config)
    {
        var conn = config.GetConnectionString("MongoDB")
                   ?? throw new InvalidOperationException("MongoDB connection string missing.");
        var dbName = config["Database:Name"] ?? "MyApiDB";
        UsersCollectionName = config["Database:UsersCollection"] ?? "Users";

        var client = new MongoClient(conn);
        Database = client.GetDatabase(dbName);
    }

    public IMongoCollection<User> Users => Database.GetCollection<User>(UsersCollectionName);
}
