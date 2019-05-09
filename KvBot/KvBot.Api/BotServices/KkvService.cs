using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KvBot.DataAccess.Contract;

namespace KvBot.Api.BotServices
{
    public class KkvService : IKkvService
    {
        private const string Add = "-add ";
        private const string IsGlobal = "-global";
        private const string As = "-as ";

        private readonly IKkvWriteCommand _kkvSaveCommand;
        private readonly IKkvFindCommand _kkvFindCommand;

        public KkvService(IKkvWriteCommand kkvSaveCommand, IKkvFindCommand kkvFindCommand)
        {
            _kkvSaveCommand = kkvSaveCommand;
            _kkvFindCommand = kkvFindCommand;
        }

        public bool TryParse(string message, 
            out (KkvScope scope, string value, string[] keys) kkv, 
            out (KkvParseResult code, string message) operationResult)
        {
            var initLength = message.Length;
            message = message.Replace(IsGlobal, string.Empty).Trim();
            KkvScope scope = message.Length == initLength ? KkvScope.Private : KkvScope.Public;

            int addPos = message.IndexOf(Add, StringComparison.OrdinalIgnoreCase);
            int asPos = message.IndexOf(As, StringComparison.OrdinalIgnoreCase);

            if (addPos < 0 && asPos < 0)
            {
                kkv = (KkvScope.Private, string.Empty, null);
                operationResult = (code: KkvParseResult.Unrecognized, message: $"{Add} was not found");
                return false;
            }

            string value = null;
            string[] keys = null;
            KkvParseResult code = KkvParseResult.Unrecognized;

            //only add command
            if (addPos >= 0 && asPos < 0)
            {
                value = message.Substring(addPos + Add.Length);
                keys = null;
                code = KkvParseResult.ValueOnly;
            }

            //both commands
            if (addPos >= 0 && asPos >= 0)
            {
                if (addPos < asPos)
                {
                    value = message.Substring(Add.Length, asPos - Add.Length);
                    keys = message.Substring(asPos + As.Length).Split(',', StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    keys = message.Substring(As.Length, addPos - As.Length).Split(',', StringSplitOptions.RemoveEmptyEntries);
                    value = message.Substring(addPos + Add.Length);
                }

                code = KkvParseResult.KeysAndValue;
            }

            //only as command
            if (addPos < 0 && asPos >= 0)
            {
                value = null;
                keys = message.Substring(asPos + As.Length).Split(',',  StringSplitOptions.RemoveEmptyEntries);
                code = KkvParseResult.KeysOnly;
            }

            kkv = (scope: scope, value: value, keys: keys);
            operationResult = (code: code, message: "Parsed successfully");

            return true;
        }

        public bool TryFindByKey(string inMessage, string userId, out string outMessage)
        {
            var sanitizedText = inMessage.Replace("?", "").Trim().ToLower();

            outMessage = _kkvFindCommand.Execute(sanitizedText, userId);

            return !string.IsNullOrEmpty(outMessage);
        }

        public async Task SaveAsync(string scope, string value, ICollection<string> keys)
        {
            await _kkvSaveCommand.ExecuteAsync(scope, value, keys);
        }
    }

    public interface IKkvService
    {
        bool TryParse(string message,
            out (KkvScope scope, string value, string[] keys) kkv,
            out (KkvParseResult code, string message) operationResult);

        Task SaveAsync(string scope, string value, ICollection<string> keys);

        bool TryFindByKey(string inMessage, string userId, out string outMessage);
    }

    public enum KkvParseResult
    {
        Unrecognized = 0,
        KeysOnly = 1,
        ValueOnly = 2,
        KeysAndValue = 3,
    }

    public enum KkvScope
    {
        Private = 0,
        Public = 1,
    }
}
