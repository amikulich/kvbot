using System.Collections.Generic;
using System.Threading.Tasks;
using KvBot.DataAccess.Contract;
using KvBot.DataAccess.Mappings;

namespace KvBot.DataAccess
{
    public class KkvWriteCommand : IKkvWriteCommand
    {
        private readonly IDbContext _dbContext;

        public KkvWriteCommand(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ExecuteAsync(string scope, string value, ICollection<string> keys)
        {
            var kkvs = new KeyKeyValue()
            {
                Keys = keys,
                Response = value,
                AlternativeResponses = new List<string>(0)
            };

            await _dbContext.SaveAsync(kkvs);
        }
    }
}
