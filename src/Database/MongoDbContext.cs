using ApiIntegrationLog.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace ApiIntegrationLog.Api.Database;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbContext>? _logger;

    public IMongoCollection<User> Users { get; }

    public MongoDbContext(
        string connectionString,
        string databaseName,
        ILogger<MongoDbContext>? logger = null
    )
    {
        var mongoConnectionUrl = new MongoUrl(connectionString);
        var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);

        if (logger is not null)
        {
            _logger = logger;

            mongoClientSettings.ClusterConfigurator =
                cb =>
                cb.Subscribe<CommandStartedEvent>(e =>
                    _logger.LogInformation($"{e.CommandName} - {e.Command.ToJson()}")
                );
        }

        var client = new MongoClient(mongoClientSettings);
        _database = client.GetDatabase(databaseName);

        Users = _database.GetCollection<User>("User");
    }
}