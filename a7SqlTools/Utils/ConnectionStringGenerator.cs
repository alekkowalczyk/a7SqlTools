using a7SqlTools.Config.Model;
using a7SqlTools.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Utils
{
    public static class ConnectionStringGenerator
    {
        public static string Get(ConnectionData data, string databaseName = null) =>
            Get(data.AuthType, data.Name, data.UserName, data.Password, databaseName);

        public static string Get(AuthType authType, string name, string userName = null, string password = null, string databaseName = null)
        {
            if (authType == AuthType.Windows)
            {
                return
                    $"MultipleActiveResultSets=True;Data Source={name};  {((databaseName != null) ? $"database={databaseName};" : "")}Integrated Security=yes;Persist Security Info=false;";
            }
            else if (authType == AuthType.Sql)
            {
                if (userName == null || password == null)
                {
                    throw new Exception("For sql authentication user name and password are required!");
                }
                return
                    $"MultipleActiveResultSets=True;Data Source={name}; {((databaseName != null) ? $"database={databaseName};" : "")}Integrated Security=False;User ID={userName};Password={password}";
            }
            else
            {
                throw new Exception("Unknown auth type:" + authType.ToString());
            }
        }
    }
}
