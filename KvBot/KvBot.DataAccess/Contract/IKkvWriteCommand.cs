using System.Collections.Generic;
using System.Threading.Tasks;

namespace KvBot.DataAccess.Contract
{
    public interface IKkvWriteCommand
    {
        Task ExecuteAsync(string scope, string value, ICollection<string> keys);
    }
}
