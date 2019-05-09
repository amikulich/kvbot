using System.Collections.Generic;

namespace KvBot.DataAccess.Mappings
{
    [Document(Name = "key-key-value")]
    internal class KeyKeyValue : MapBase
    {
        public KeyKeyValue()
        {
            Keys = new List<string>();
            AlternativeResponses = new List<string>();
        }

        public string Response { get; set; }

        public IEnumerable<string> Keys { get; set; }

        public IEnumerable<string> AlternativeResponses { get; set; }

        public string Scope { get; set; }
    }
}
