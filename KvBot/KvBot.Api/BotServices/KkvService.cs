using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KvBot.DataAccess.Contract;

namespace KvBot.Api.BotServices
{
    public class KkvService : IKkvService
    {
        private const string Add = "-ADD ";
        private const string As = "-AS ";

        private readonly IKkvWriteCommand _kkvSaveCommand;

        public KkvService(IKkvWriteCommand kkvSaveCommand)
        {
            _kkvSaveCommand = kkvSaveCommand;
        }

        public bool TryParse(string message, 
            out (string scope, string value, string[] keys) kkv, 
            out (KkvParserCodes code, string message) operationResult)
        {
            int addPos = message.IndexOf(Add, StringComparison.OrdinalIgnoreCase);
            int asPos = message.IndexOf(As, StringComparison.OrdinalIgnoreCase);

            if (addPos < 0 && asPos < 0)
            {
                kkv = (string.Empty, string.Empty, null);
                operationResult = (code: KkvParserCodes.Unrecognized, message: $"{Add} was not found");
                return false;
            }

            string value = null;
            string[] keys = null;
            KkvParserCodes code = KkvParserCodes.Unrecognized;

            //only add command
            if (addPos >= 0 && asPos < 0)
            {
                value = message.Substring(addPos + Add.Length);
                keys = null;
                code = KkvParserCodes.ValueOnly;
            }

            //both commands
            if (addPos >= 0 && asPos >= 0)
            {
                if (addPos < asPos)
                {
                    value = message.Substring(Add.Length, asPos - Add.Length);
                    keys = message.Substring(asPos + As.Length).Split(',');
                }
                else
                {
                    keys = message.Substring(As.Length, addPos - As.Length).Split(',');
                    value = message.Substring(addPos + Add.Length);
                }

                code = KkvParserCodes.KeysAndValue;
            }

            //only as command
            if (addPos < 0 && asPos >= 0)
            {
                value = null;
                keys = message.Substring(asPos + As.Length).Split(',');
                code = KkvParserCodes.KeysOnly;
            }

            kkv = (scope: "private", value: value, keys: keys);
            operationResult = (code: code, message: "Parsed successfully");

            return true;
        }

        public async Task SaveAsync(string scope, string value, ICollection<string> keys)
        {
            await _kkvSaveCommand.ExecuteAsync(scope, value, keys);
        }
    }

    public interface IKkvService
    {
        bool TryParse(string message,
            out (string scope, string value, string[] keys) kkv,
            out (KkvParserCodes code, string message) operationResult);

        Task SaveAsync(string scope, string value, ICollection<string> keys);
    }

    public enum KkvParserCodes
    {
        Unrecognized = 0,
        KeysOnly = 1,
        ValueOnly = 2,
        KeysAndValue = 3,
    }
}
