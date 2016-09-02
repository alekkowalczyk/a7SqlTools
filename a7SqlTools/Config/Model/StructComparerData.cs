using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace a7SqlTools.Config.Model
{
    public class StructComparerData : IWithCreatedAt
    {
        [JsonProperty("dbA")]
        public string DbA { get; set; }

        [JsonProperty("dbB")]
        public string DbB { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
