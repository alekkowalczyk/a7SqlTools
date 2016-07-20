using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Config.Model
{
    public class LocalConfig
    {
        [JsonProperty("connections")]
        public List<ConnectionItem> Connections { get; set; }

        public LocalConfig()
        {
            Connections = new List<ConnectionItem>();
        }
    }
}
