using a7SqlTools.Connection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7SqlTools.Utils;

namespace a7SqlTools.Config.Model
{
    public class ConnectionData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("authType")]
        public AuthType AuthType { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("dbSearches")]
        public List<DbSearchData> DbSearches { get; set; }

        [JsonProperty("tableExplorers")]
        public List<TableExplorerData> TableExplorers { get; set; }

        public ConnectionData()
        {
            DbSearches = new List<DbSearchData>();
            TableExplorers = new List<TableExplorerData>();
        }
    }
}
