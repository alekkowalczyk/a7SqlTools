using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Windows.Threading;
using System.Windows.Input;
using a7SqlTools.DbSearch;
using a7SqlTools.TableExplorer;

namespace a7SqlTools.Controls.DataGrids
{
    /// <summary>
    /// A grid that makes inline filtering possible.
    /// </summary>
    public class a7DataGrid : DataGrid
    {
        /// <summary>
        /// This dictionary will have a list of all applied filters
        /// </summary>
        private Dictionary<string, string> columnFilters;
        private Dictionary<string, TextBox> columnFiltersTextBoxes;
        /// <summary>
        /// Cache with properties for better performance
        /// </summary>
        private Dictionary<string, PropertyInfo> propertyCache;
        public ICommand CellShow { get; set; }
        /// <summary>
        /// Case sensitive filtering
        /// </summary>
        public static DependencyProperty IsFilteringCaseSensitiveProperty =
             DependencyProperty.Register("IsFilteringCaseSensitive", typeof(bool), typeof(a7DataGrid), new PropertyMetadata(true));

        /// <summary>
        /// Case sensitive filtering
        /// </summary>
        public bool IsFilteringCaseSensitive
        {
            get { return (bool)(GetValue(IsFilteringCaseSensitiveProperty)); }
            set { SetValue(IsFilteringCaseSensitiveProperty, value); }
        }

        public static DependencyProperty TableExplorerProperty =
            DependencyProperty.Register("TableExplorer", typeof(a7SingleTableExplorer), typeof(a7DataGrid), 
            new PropertyMetadata(new PropertyChangedCallback(TableExplorerPropertyChanged)));

        public a7SingleTableExplorer TableExplorer
        {
            get { return (a7SingleTableExplorer)GetValue(TableExplorerProperty); }
            set { SetValue(TableExplorerProperty, value); }
        }

        public static DependencyProperty IsFilterEnabledProperty =
            DependencyProperty.Register("IsFilterEnabled", typeof(bool), typeof(a7DataGrid),
            new PropertyMetadata(new PropertyChangedCallback(TableExplorerPropertyChanged)));

        public a7SingleTableExplorer IsFilterEnabled
        {
            get { return (a7SingleTableExplorer)GetValue(IsFilterEnabledProperty); }
            set { SetValue(IsFilterEnabledProperty, value); }
        }

        /// <summary>
        /// Register for all text changed events
        /// </summary>
        public a7DataGrid()
        {
            // Initialize lists
            columnFilters = new Dictionary<string, string>();
            propertyCache = new Dictionary<string, PropertyInfo>();
            // Add a handler for all text changes
            CellShow = new CellShowCommand();
            AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnTextChanged), true);
            // Datacontext changed, so clear the cache
            DataContextChanged += new DependencyPropertyChangedEventHandler(FilteringDataGrid_DataContextChanged);
            CellEditEnding += new EventHandler<DataGridCellEditEndingEventArgs>(a7DataGrid_CellEditEnding);

        }

        void a7DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Clear the property cache if the datacontext changes.
        /// This could indicate that an other type of object is bound.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilteringDataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            propertyCache.Clear();

        }


        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            SynchronizeHeaderTextBoxes();
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(()=>{SynchronizeHeaderTextBoxes();}));
        }

        private static void TableExplorerPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            a7DataGrid fdg = o as a7DataGrid;
            if(fdg != null)
            {
                a7SingleTableExplorer gte = e.NewValue as a7SingleTableExplorer;
                if (gte != null)
                {
                    gte.FilterFields = fdg.columnFilters;
                }
            }
        }

        /// <summary>
        /// When a text changes, it might be required to filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the textbox
            TextBox filterTextBox = e.OriginalSource as TextBox;

            // Get the header of the textbox
            DataGridColumnHeader header = TryFindParent<DataGridColumnHeader>(filterTextBox);
            if (header != null)
            {
                UpdateFilter(filterTextBox, header);
                ApplyFilters();
            }
        }

        /// <summary>
        /// Update the internal filter
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="header"></param>
        private void UpdateFilter(TextBox textBox, DataGridColumnHeader header)
        {
            // Try to get the property bound to the column.
            // This should be stored as datacontext.
            string columnBinding = header.DataContext != null ? header.DataContext.ToString() : "";

            // Set the filter 
            if (!String.IsNullOrEmpty(columnBinding))
            {
                  columnFilters[columnBinding] = textBox.Text;
         //       textBox.SetBinding(TextBox.TextProperty, new Binding("[" + columnBinding + "]") { Source = this.columnFilters });
            }
        }

        /// <summary>
        /// Apply the filters
        /// </summary>
        /// <param name="border"></param>
        private void ApplyFilters()
        {
            // Get the view
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
            if (view != null)
            {
                if (this.TableExplorer != null)
                {
                    this.TableExplorer.FilterFields = this.columnFilters;
      //              this.TableExplorer.Filter2Sql();
                }
                else
                {
                    DataView dv = view.SourceCollection as DataView;
                    if (dv != null)
                    {
                        string rowFilter = "";
                        bool isFirst = true;
                        foreach (KeyValuePair<string, string> kv in this.columnFilters)
                        {
                            if(isFirst)
                                isFirst=false;
                            else
                                rowFilter += " AND ";
                            rowFilter += " " + kv.Key + " LIKE '%" + kv.Value + "%' ";
                        }
                        dv.RowFilter = rowFilter;
                    }
                }
            }
        }

        /// <summary>
        /// Get the value of a property
        /// </summary>
        /// <param name="item"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private object GetPropertyValue(object item, string property)
        {
            // No value
            object value = null;

            // Get property  from cache
            PropertyInfo pi = null;
            if (propertyCache.ContainsKey(property))
                pi = propertyCache[property];
            else
            {
                pi = item.GetType().GetProperty(property);
                propertyCache.Add(property, pi);
            }

            // If we have a valid property, get the value
            if (pi != null)
                value = pi.GetValue(item, null);

            // Done
            return value;
        }

        public void SynchronizeHeaderTextBoxes()
        {
            DataGridColumnHeadersPresenter presenter = null;

            Control sv = this.Template.FindName("DG_ScrollViewer", this) as Control;
            if (sv != null)
            {
                presenter = sv.Template.FindName("PART_ColumnHeadersPresenter", sv) as DataGridColumnHeadersPresenter;
            }

            DataGridColumnHeader header = null;
            if (presenter != null)
            {
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    header = (DataGridColumnHeader)presenter.ItemContainerGenerator.ContainerFromIndex(i);
                    if (header != null)
                    {
                        TextBox tb = FindVisualChildByName<TextBox>(header, "filterTextBox");
                        string columnName = header.Column.Header.ToString();
                        if (this.columnFilters.ContainsKey(columnName))
                            tb.Text = this.columnFilters[columnName];
                    }
                }
            }
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null reference is being returned.</returns>
        public static T TryFindParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Do note, that for content element,
        /// this method falls back to the logical tree of the element.
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise null.</returns>
        public static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;

            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            // If it's not a ContentElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T>(child, name);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

    }

    public class CellShowCommand : ICommand
    {

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
