using KvBot.DataAccess.Contract;
using Microsoft.Bot.Schema;

namespace KvBot.Api.BotServices
{
    public class SimpleResolver : ISimpleResolver
    {
        private readonly IPredefinedCommandQuery _predefinedCommandQuery;

        public SimpleResolver(IPredefinedCommandQuery predefinedCommandQuery)
        {
            _predefinedCommandQuery = predefinedCommandQuery;
        }

        public bool TryResolve(Activity activity, out string message)
        {
            message = _predefinedCommandQuery.Execute(activity.Text.Replace("?", "").Trim().ToLower());

            return !string.IsNullOrEmpty(message);
        }
    }

    public interface ISimpleResolver
    {
        bool TryResolve(Activity activity, out string message);
    }
}
