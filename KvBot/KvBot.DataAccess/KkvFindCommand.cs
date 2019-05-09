using System;
using System.Linq;
using KvBot.DataAccess.Contract;
using KvBot.DataAccess.Mappings;

namespace KvBot.DataAccess
{
    public class KkvFindCommand : IKkvFindCommand
    {
        private readonly IDbContext _dbContext;

        public KkvFindCommand(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static readonly Random Random = new Random();

        public string Execute(string key, string userId)
        {
            IQueryable<KeyKeyValue> kkvs = _dbContext
                    .AsQueryable<KeyKeyValue>()
                    .Where(kkv => kkv.Keys.Contains(key) 
                                  && (string.IsNullOrEmpty(kkv.Scope) || kkv.Scope == userId));

            if (!kkvs.Any())
            {
                return string.Empty;
            }

            KeyKeyValue keyKeyValue = kkvs.FirstOrDefault(kkv => kkv.Scope == userId) ?? kkvs.First();

            if (keyKeyValue.AlternativeResponses.Count() > 1)
            {
                var number = Random.Next(-1, keyKeyValue.AlternativeResponses.Count());

                return number == -1 ? keyKeyValue.Response : keyKeyValue.AlternativeResponses.ElementAt(number);
            }

            return keyKeyValue.Response;
        }
    }
}
