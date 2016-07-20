using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Config.Model
{
    public class DbSearchData : IWithCreatedAt
    {
        [JsonProperty("databaseName")]
        public string DatabaseName { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
