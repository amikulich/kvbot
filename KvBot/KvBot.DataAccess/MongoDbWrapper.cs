using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KvBot.DataAccess
{
    public interface IDbContext
    {
        IQueryable<T> AsQueryable<T>() where T : MapBase, new();

        Task SaveAsync<T>(T item) where T : MapBase, new();

        Task DeleteAsync<T>(T item) where T : MapBase, new();
    }

    public class DbContext : IDbContext
    {
        private readonly IMongoDatabase _database;

        public DbContext(string connectionString, string database)
        {
            IMongoClient client = new MongoClient(connectionString);
            _database = client.GetDatabase(database);
        }

        public IQueryable<T> AsQueryable<T>() where T : MapBase, new()
        {
            return _database.GetCollection<T>(GetDocumentNameFromType(typeof(T)))
                .AsQueryable();
        }

        public async Task SaveAsync<T>(T item) where T : MapBase, new()
        {
            var collection = _database.GetCollection<T>(GetDocumentNameFromType(typeof(T)));

            var filter = Builders<T>.Filter.Eq(x => x.Id, item.Id);
            var result = collection.Find(filter);

            if (result.Any())
            {
                await collection.ReplaceOneAsync(filter, item);
            }
            else
            {
                await collection.InsertOneAsync(item);
            }
        }

        public async Task DeleteAsync<T>(T item) where T : MapBase, new()
        {
            var collection = _database.GetCollection<T>(GetDocumentNameFromType(typeof(T)));

            await collection.DeleteOneAsync(x => x.Id == item.Id);
        }

        private string GetDocumentNameFromType(Type type)
        {
            var attributes = type.GetCustomAttributes(false);

            var documentAttribute = (DocumentAttribute) attributes.SingleOrDefault(attr => attr is DocumentAttribute);
            if (string.IsNullOrEmpty(documentAttribute?.Name))
            {
                throw new NotImplementedException("Mapping to the document collection is missing");
            }

            return documentAttribute.Name;
        }
    }

    public abstract class MapBase
    {
        protected MapBase()
        {
        }

        public ObjectId Id { get; set; }
    }

    public class DocumentAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
