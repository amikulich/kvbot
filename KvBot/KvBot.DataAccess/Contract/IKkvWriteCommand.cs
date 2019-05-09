using System.Collections.Generic;
using System.Threading.Tasks;

namespace KvBot.DataAccess.Contract
{
    public interface IKkvWriteCommand
    {
        Task ExecuteAsync(string userId, string value, ICollection<string> keys);
    }
}
