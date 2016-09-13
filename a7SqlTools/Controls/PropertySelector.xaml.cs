using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using a7SqlTools.TableExplorer;
using a7SqlTools.TableExplorer.Enums;
using ColumnDefinition = System.Windows.Controls.ColumnDefinition;

namespace a7SqlTools.Controls
{
    /// <summary>
    /// Interaction logic for PropertySelector.xaml
    /// </summary>
    public partial class PropertySelector : UserControl
    {
        public event EventHandler SelectedPropertyChanged;

        public ObservableCollection<TableExplorer.ColumnDefinition> Properties
        {
            get { return (ObservableCollection<TableExplorer.ColumnDefinition>)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Properties.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(ObservableCollection<TableExplorer.ColumnDefinition>), typeof(PropertySelector), new PropertyMetadata(null, (s, e) =>
            {
            }));

        public TableExplorer.ColumnDefinition SelectedProperty
        {
            get { return (TableExplorer.ColumnDefinition)GetValue(SelectedPropertyProperty); }
            set { SetValue(SelectedPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPropertyProperty =
            DependencyProperty.Register("SelectedProperty", typeof(TableExplorer.ColumnDefinition), typeof(PropertySelector), new PropertyMetadata(null));

        public PropertySelector()
        {
            InitializeComponent();
            acbProperty.ItemFilter = (t, obj) => string.IsNullOrWhiteSpace(t) || obj?.ToString().IndexOf(t?.Trim(), StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if(e.Property == SelectedPropertyProperty)
            {
                this.acbProperty.Text = this.SelectedProperty.Name;
            }
        }

        public void Clear()
        {
            this.acbProperty.Text = "";
            this.acbProperty.SelectedItem = null;
        }

        private void selectedPropertychanged() => SelectedPropertyChanged?.Invoke(this, null);


        private void AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TableExplorer.ColumnDefinition)
            {
                this.SelectedProperty = e.AddedItems[0] as TableExplorer.ColumnDefinition;
                selectedPropertychanged();
            }
        }

        private void AcbProperty_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var abc = sender as AutoCompleteBox;
            if (string.IsNullOrEmpty(abc.Text))
            {
                abc.Text = " "; // when empty, we put a space in the box to make the dropdown appear
            }
            abc.Dispatcher.BeginInvoke(new Action(() => abc.IsDropDownOpen = true));
        }

        private void AcbProperty_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var abc = sender as AutoCompleteBox;
            abc.Text = abc.Text.Trim();
            var matching =
                Properties.FirstOrDefault(
                    prop => prop.Name.IndexOf(abc.Text, StringComparison.InvariantCultureIgnoreCase) > -1);
            if (matching != null)
                abc.SelectedItem = matching;
            else
            {
                abc.Text = "";
                abc.SelectedItem = null;
            }
        }

        private void AcbProperty_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var abc = sender as AutoCompleteBox;
            abc.IsDropDownOpen = true;
        }

        private void AcbProperty_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var abc = sender as AutoCompleteBox;
            if (!string.IsNullOrWhiteSpace(abc.Text) &&
              abc.FilterMode != AutoCompleteFilterMode.Custom)
            {
                abc.FilterMode = AutoCompleteFilterMode.Custom;
            }

            if (string.IsNullOrWhiteSpace(abc.Text) &&
                abc.FilterMode != AutoCompleteFilterMode.None)
            {
                abc.FilterMode = AutoCompleteFilterMode.None;
            }
        }

        private void AcbProperty_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var abc = sender as AutoCompleteBox;
            //if (abc.Text != " ")
            //    abc.Text = abc.Text.Trim();
        }
    }
}
