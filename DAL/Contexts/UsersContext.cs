using DAL.Models;
using DAL.Models.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DAL.Contexts
{
    public class UsersContext
    {
        private readonly IMongoDatabase _database = null;

        public UsersContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<User> Users
        {
            get
            {
                return _database.GetCollection<User>("UsersRabbit");
            }
        }
    }
}