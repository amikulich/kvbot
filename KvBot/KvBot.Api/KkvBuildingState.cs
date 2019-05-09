using System.Collections.Generic;
using KvBot.Api.BotServices;

namespace KvBot.Api
{
    public class KkvBuildingState
    {
        private readonly List<string> _keys;

        public KkvScope Scope { get; set; }

        public KkvBuildingState()
        {
            _keys = new List<string>();
            Value = string.Empty;
            Scope = KkvScope.Private;
        }

        public void SetScope(KkvScope scope)
        {
            Scope = scope;
        }

        public void SetValue(string value)
        {
            Value = value;
        }

        public void SetKeys(string[] keys)
        {
            _keys.AddRange(keys);
        }

        public ICollection<string> Keys => _keys;

        public string Value { get; set; }

        public bool IsTransient()
        {
            return string.IsNullOrEmpty(Value)
                   && _keys?.Count == 0;
        }

        public bool IsCommitable()
        {
            return !string.IsNullOrEmpty(Value)
                   && _keys?.Count > 0;
        }
    }
}
