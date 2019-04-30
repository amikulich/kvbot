using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KvBot.DataAccess
{
    internal class MongoDriverWrapper : IDisposable
    {
        static readonly IMongoClient Client = new MongoClient("mongodb://f1bot:1qaz!QAZ@ds050539.mlab.com:50539/f1bot-db");
        static readonly IMongoDatabase Database = Client.GetDatabase("f1bot-db");

        public void Dispose()
        {
        }

        public IQueryable<T> All<T>() where T : MapBase, new()
        {
            return Database.GetCollection<T>(GetDocumentNameFromType(typeof(T)))
                .AsQueryable();
        }

        public async Task SaveAsync<T>(T item) where T : MapBase, new()
        {
            var collection = Database.GetCollection<T>(GetDocumentNameFromType(typeof(T)));

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

    internal abstract class MapBase
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
