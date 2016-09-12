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
    /// Interaction logic for ValuesDBSearch.xaml
    /// </summary>
    public partial class ValuesDBSearch : UserControl
    {
        public a7DbSearchEngine DBSearch => DataContext as a7DbSearchEngine;

        public ValuesDBSearch()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(UserControl1_DataContextChanged);
        }


        void UserControl1_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DBSearch == null) return;
            DBSearch.ActualizedWork += new EventHandler<a7DbSearchEngine.DBSearchEventArgs>(dbSearch_AcutalizedWork);
            DBSearch?.RefreshDictTables(this.tbTableFilter.Text);
        }


        private void dbSearch_AcutalizedWork(object sender, a7DbSearchEngine.DBSearchEventArgs e)
        {
            this.tbProgress.Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        if (e != null)
                        {
                            this.tbProgress.Text = "Table:" + e.ActualAnalizedTable + "(" + e.ActualTable + "/" +
                                                   e.TableCount + ")" +
                                                   ",  Value:" + "(" + e.ActualTableValue + "/" + e.ValuesCount + ")" +
                                                   e.ActualAnalizedValue;
                        }
                        else
                        {
                            this.tbProgress.Text = "";
                        }
                    }
                    )
                );
            this.pbProgress.Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        if (e != null)
                        {
                            this.pbProgress.Value =
                            ((double)
                             (((double) e.ValuesCount*e.ActualTable) - (e.ValuesCount - e.ActualTableValue))/
                             ((double) e.TableCount*e.ValuesCount))*100;
                        }
                        else
                        {
                            this.pbProgress.Value = 0;
                        }
                    }
                    )
                );
        }


        private void SelectAllTables_Checked(object sender, RoutedEventArgs e)
        {
            if (DBSearch != null)
            {
                DBSearch.SelectAllTables(true);
                lbTables.Items.Refresh();
            }
        }

        private void SelectAllTables_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DBSearch != null)
            {
                DBSearch.SelectAllTables(false);
                lbTables.Items.Refresh();
            }
        }

        private void bSearchDB_Click(object sender, RoutedEventArgs e)
        {
            this.pbProgress.Value = 0;
            DBSearch.BeginSearchValues(this.tbValuesToSearch.Text);
        }


        private void bAbortSearch_Click(object sender, RoutedEventArgs e)
        {
            this.DBSearch.AbortSearch();        
        }


        private void lbValuesSearched_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                DBSearch.SelectSearchedValue(e.AddedItems[0] as a7SearchedValue);
        }

        private void lbTablesWithValueFound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is KeyValuePair<string, int>)
                DBSearch.SelectTable(((KeyValuePair<string, int>)e.AddedItems[0]).Key);
        }

        private void tbTableFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(DBSearch!=null)
                DBSearch.RefreshDictTables(this.tbTableFilter.Text);
        }
        
        private void bCommit_Click(object sender, RoutedEventArgs e)
        {
            DBSearch.CommitSelectedTable();
        }

        private void BBack_OnClick(object sender, RoutedEventArgs e)
        {
            DBSearch.IsResultsView = false;
        }

        private void BSetSeperators_OnClick(object sender, RoutedEventArgs e)
        {
            DBSearch.SetSeperators();
        }
    }
}
