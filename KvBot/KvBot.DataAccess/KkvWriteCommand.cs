using System.Collections.Generic;
using System.Threading.Tasks;
using KvBot.DataAccess.Contract;
using KvBot.DataAccess.Mappings;

namespace KvBot.DataAccess
{
    public class KkvWriteCommand : IKkvWriteCommand
    {
        public async Task ExecuteAsync(string scope, string value, ICollection<string> keys)
        {
            var kkvs = new PredefinedCommandMap()
            {
                Keys = keys,
                Response = value,
                AlternativeResponses = new List<string>(0)
            };

            using (var dbContext = new MongoDriverWrapper())
            {
                await dbContext.SaveAsync(kkvs);
            }
        }
    }
}
