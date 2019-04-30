using System.Collections.Generic;

namespace KvBot.Api
{
    public class KkvBuildingState
    {
        private readonly List<string> _keys;
        private string _value;

        public KkvBuildingState()
        {
            _keys = new List<string>();
        }

        public void SetValue(string value)
        {
            _value = value;
        }

        public void SetKeys(string[] keys)
        {
            _keys.AddRange(keys);
        }

        public ICollection<string> Keys => _keys;

        public string Value => _value;

        public bool IsTransient()
        {
            return string.IsNullOrEmpty(_value)
                   && _keys?.Count == 0;
        }

        public bool IsCommitable()
        {
            return !string.IsNullOrEmpty(_value)
                   && _keys?.Count > 0;
        }
    }
}
