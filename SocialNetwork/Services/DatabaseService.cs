// Services/DatabaseService.cs
using MongoDB.Driver;
using SocialNetwork.Models;

namespace SocialNetwork.Services
{
    
    public class DatabaseService
    {
        private readonly IMongoDatabase _database;
        private const string ConnectionString = "mongodb://localhost:27017";
        private const string DatabaseName = "social_network_new"; 

        public DatabaseService()
        {
            var client = new MongoClient(ConnectionString);
            _database = client.GetDatabase(DatabaseName);

            EnsureIndexes();
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        private void EnsureIndexes()
        {
           
            var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var emailIndexModel = new CreateIndexModel<User>(emailIndex, new CreateIndexOptions { Unique = true });
            Users.Indexes.CreateOne(emailIndexModel);

            var postIdIndex = Builders<User>.IndexKeys.Ascending("Posts.PostId");
            var postIdIndexModel = new CreateIndexModel<User>(postIdIndex);
            Users.Indexes.CreateOne(postIdIndexModel);
        }
    }
}