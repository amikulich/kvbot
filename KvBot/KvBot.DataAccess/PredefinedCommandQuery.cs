using System;
using System.Linq;
using KvBot.DataAccess.Contract;
using KvBot.DataAccess.Mappings;

namespace KvBot.DataAccess
{
    public class PredefinedCommandsQuery : IPredefinedCommandQuery
    {
        private readonly IDbContext _dbContext;

        public PredefinedCommandsQuery(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static readonly Random Random = new Random();

        public string Execute(string key)
        {
            var map = _dbContext
                .All<KeyKeyValue>()
                .FirstOrDefault(x => x.Keys.Contains(key));

            if (map != null && map.AlternativeResponses.Any())
            {
                var number = Random.Next(-1, map.AlternativeResponses.Count());

                return number == -1 ? map.Response : map.AlternativeResponses.ElementAt(number);
            }

            return map?.Response;
        }
    }
}
