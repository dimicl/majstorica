using MongoDB.Driver;

namespace backend.Data;

public class MongoDBService {
    private readonly IConfiguration _configuration;
    private readonly IMongoDatabase _database;

    public MongoDBService(IConfiguration configuration) {
        _configuration = configuration;

        var connectionString = _configuration.GetConnectionString("DbConnection");
        var mongourl = MongoUrl.Create(connectionString);
        var mongoClient = new MongoClient(mongourl);
        _database = mongoClient.GetDatabase(mongourl.DatabaseName);
        
    }

    public IMongoDatabase? Database => _database;
}