using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7SqlTools.Config.Model;

namespace a7SqlTools.Utils
{
    static class a7DbUtils
    {
        public static async Task<List<string>> GetDbNames(ConnectionData connData)//ServerConnection connection)
        {
            var list = new List<string>();
            var conStrng = ConnectionStringGenerator.Get(connData);
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection con = new SqlConnection(conStrng))
                {
                    con.Open();

                    // Set up a command with the given query and associate
                    // this with the current connection.
                    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var dbName = dr[0].ToString();
                                if(dbName != "master" && dbName != "tempdb" && dbName != "model" && dbName != "msdb") // ugly way to exclude system databases
                                    list.Add(dbName);
                            }
                        }
                    }
                }
            });
            return list;
        }


    }
}
