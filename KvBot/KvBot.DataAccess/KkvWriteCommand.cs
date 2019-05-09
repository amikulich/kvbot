using System.Collections.Generic;
using System.Linq;
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

        public async Task ExecuteAsync(string userId, string value, ICollection<string> keys)
        {
            foreach (var key in keys)
            {
                var existingKey = _dbContext.AsQueryable<KeyKeyValue>()
                    .FirstOrDefault(kkv => kkv.Keys.Contains(key) && kkv.Scope == userId);

                if (existingKey != null)
                {
                    await _dbContext.DeleteAsync(existingKey);
                }
            }

            var kkvs = new KeyKeyValue()
            {
                Keys = keys,
                Response = value,
                AlternativeResponses = new List<string>(0),
                Scope = userId
            };

            await _dbContext.SaveAsync(kkvs);
        }
    }
}
