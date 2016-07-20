using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Config.Model
{
    public class ConnectionItem
    {
        [JsonProperty("ConnectionData")]
        public ConnectionData ConnectionData { get; set; }

        [JsonProperty("IsExpanded")]
        public bool IsExpanded { get; set; }
    }
}
