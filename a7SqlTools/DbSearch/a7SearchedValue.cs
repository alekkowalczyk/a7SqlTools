using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace a7SqlTools.DbSearch
{
    public class a7SearchedValue
    {
        public string Value { get; private set; }
        public int FoundOccurrences { get; private set; }
        public Dictionary<string, int> OccurencesInTables { get; private set; }
        public DataSet SearchResultSet { get; private set; }
        Dictionary<string, SqlDataAdapter> adapters;

        public a7SearchedValue(string name)
        {
            Value = name;
            OccurencesInTables = new Dictionary<string, int>();
            adapters = new Dictionary<string, SqlDataAdapter>();
            SearchResultSet = new DataSet();
            FoundOccurrences = 0;
        }

        public void AddDataTable(DataTable dt, SqlDataAdapter adapter)
        {
            if (dt.Rows.Count > 0)
            {
                SearchResultSet.Tables.Add(dt);
                OccurencesInTables.Add(dt.TableName, dt.Rows.Count);
                adapters.Add(dt.TableName, adapter);
                FoundOccurrences += dt.Rows.Count;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value + " (" + FoundOccurrences.ToString() + ")";
        }

        public void CommitUpdates(string tableName)
        {
            try
            {
                var ret = adapters[tableName].Update(SearchResultSet, tableName);
             //   adapters[tableName].Fill(SearchResultSet, tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
