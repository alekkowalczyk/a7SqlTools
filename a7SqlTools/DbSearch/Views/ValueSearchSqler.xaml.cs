using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace a7SqlTools.DbSearch.Views
{
    /// <summary>
    /// Interaction logic for ValueSearchSqler.xaml
    /// </summary>
    public partial class ValueSearchSqler : UserControl
    {
        public a7DbSearchEngine DBSearch
        {
            get
            {
                if (DataContext is a7DbSearchEngine)
                    return DataContext as a7DbSearchEngine;
                else return null;
            }
        }

        public ValueSearchSqler()
        {
            InitializeComponent();
        }

        public void ParseQuery(string value2replace)
        {
            tbQueryParsed.Text = tbQueryToParse.Text.Replace("&&&", value2replace);
        }

        private void bRunSqler_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = this.tbQueryParsed.Text;
                string ret = "";
                if (sql.ToLower().Trim().StartsWith("select"))
                    ret = this.DBSearch.GetValue(sql);
                else
                    ret = this.DBSearch.ExecuteSQL(sql).ToString() + " rows affected.";                   
                this.lSqlerOutput.Text = ret;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
    }
}
