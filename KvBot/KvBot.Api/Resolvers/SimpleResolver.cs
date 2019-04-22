using KvBot.DataAccess;
using Microsoft.Bot.Schema;

namespace KvBot.Api.Resolvers
{
    public class SimpleResolver : ISimpleResolver
    {
        private readonly PredefinedCommandsQuery _predefinedCommandsQuery = new PredefinedCommandsQuery();

        public bool TryResolve(Activity activity, out string message)
        {
            message = _predefinedCommandsQuery.Execute(activity.Text.Replace("?", "").Trim().ToLower());

            return !string.IsNullOrEmpty(message);
        }
    }

    public interface ISimpleResolver
    {
        bool TryResolve(Activity activity, out string message);
    }
}
