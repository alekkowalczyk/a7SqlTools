using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using a7SqlTools.TableExplorer;
using a7SqlTools.TableExplorer.Enums;
using a7SqlTools.TableExplorer.Filter;
using a7SqlTools.Utils;
using ColumnDefinition = a7SqlTools.TableExplorer.ColumnDefinition;

namespace a7SqlTools.Controls.Filter
{
    /// <summary>
    /// Interaction logic for a7FilterAtomEditor.xaml
    /// </summary>
    public partial class a7FilterElementEditor : UserControl
    {
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(a7FilterElementEditor), new UIPropertyMetadata(false));


        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(FilterExpressionData), typeof(a7FilterElementEditor), new PropertyMetadata(default(FltAtomExprData)));

        public FilterExpressionData Filter
        {
            get { return (FilterExpressionData)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        public a7SqlTools.Controls.Filter.a7FilterEditor EditorContext { get; set; }
        public bool IsWithEntityIdFilter { get; private set; }
        private FrameworkElement _frameworkElement;

        public a7FilterElementEditor(a7FilterElementDefinition elementDef)
        {
            InitializeComponent();
            IsWithEntityIdFilter = false;

            var field = elementDef.FieldData;
            Filter = new FltAtomExprData()
            {
                Operator = FilterFieldOperator.Equal,
                Field = field.Name
            };
            setField(field);
        }

        public a7FilterElementEditor(a7SingleTableExplorer entity, FltAtomExprData filter)
        {
            InitializeComponent();
            var field = entity.AvailableColumns.FirstOrDefault(c => c.Name == filter.Field);
            Filter = filter;
            IsWithEntityIdFilter = false;
            setField(field);
        }

        public void FocusControl()
        {
            if (this.spMain.Children.Count > 1)
            {
                var _popupTimer = new DispatcherTimer(DispatcherPriority.Normal);
                var ctrl = this.spMain.Children[1];
                _popupTimer.Interval = TimeSpan.FromMilliseconds(100);
                _popupTimer.Tick += (obj, e) =>
                {
                    ctrl.Dispatcher.Invoke(new Action(() => ctrl.Focus()));
                    _popupTimer.Stop();
                };
                _popupTimer.Start();
            }
        }

        private void setField(ColumnDefinition field)
        {
            this.lCaption.Content = field.Name;
            FrameworkElement fe;
            if (field.Type == PropertyType.Bool)
                fe = getBoolFilter(field);
            else if (field.Type == PropertyType.DateTime)
                fe = getDatePicker(field);
            else
                fe = getTextBox(field);
            fe.Margin = new Thickness(0);
            spMain.Children.Add(fe);
            _frameworkElement = fe;
        }

        private a7FilterTextBox getTextBox(ColumnDefinition field)
        {
            var ftb = new a7FilterTextBox(field) { Height = 18, Padding = new Thickness(0), Width = 120 };
      //      ftb.FilterType = (this.Filter as a7FltAtomExprData).Operator;
            ftb.SetBinding(a7FilterTextBox.TextProperty, getFilterValueBinding());
            var faExpr = this.Filter as FltAtomExprData;
            if (faExpr != null && faExpr.Value.IsEmpty())
            {
                if (field.Type == PropertyType.String)
                    (this.Filter as FltAtomExprData).Operator = FilterFieldOperator.Contains;
                else
                    (this.Filter as FltAtomExprData).Operator = FilterFieldOperator.Equal;
            }
            var operatorBinding = new Binding("Operator")
            {
                Source = this.Filter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(a7FilterTextBox.FilterTypeProperty, operatorBinding);
            ftb.SetBinding(a7FilterTextBox.IsEnabledProperty, this.getIsEnabledBinding());
            ftb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");
            ftb.KeyUp += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    activateFilter();
            };
            return ftb;
        }

     
        private a7FilterDatePicker getDatePicker(ColumnDefinition field)
        {
            a7FilterDatePicker ftb = new a7FilterDatePicker();
            ftb.HasTime = false;
            ftb.Padding = new Thickness(0.0);
            ftb.Margin = new Thickness(0.0);
            ftb.Width = 120;
            ftb.HorizontalAlignment = HorizontalAlignment.Stretch;
            ftb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");
            var fa = this.Filter as FltAtomExprData;
            fa.Operator = FilterFieldOperator.Between;
            //if(fa!=null)
            //    fa.Operator = a7FilterFieldOperator.Equal;
            ftb.SetBinding(a7FilterDatePicker.TextProperty, getFilterValueBinding());

            ftb.Height = 18;
            ftb.FontSize = 12;
            var operatorBinding = new Binding("Operator")
            {
                Source = this.Filter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(a7FilterDatePicker.FilterTypeProperty, operatorBinding);
            ftb.SetBinding(a7FilterDatePicker.IsEnabledProperty, getIsEnabledBinding());
            ftb.KeyUp += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                        activateFilter();
                };
            return ftb;
        }

        class comboItem
        {
            public string Value { get; private set; }
            public string Name { get; private set; }
            public comboItem(string value, string name)
            {
                Value = value;
                Name = name;
            }
        }

        private a7ComboBox getBoolFilter(ColumnDefinition field)
        {
            a7ComboBox cb = new a7ComboBox();
            cb.SelectedValuePath = "Value";
            cb.DisplayMemberPath = "Name";
            cb.Width = 120;
            var items = new ObservableCollection<comboItem>();
            items.Add(new comboItem("1", "True"));
            items.Add(new comboItem("0", "False"));
            cb.ItemsSource = items;
            cb.Background = Brushes.Red;
            var template = ResourcesManager.Instance.GetControlTemplate("CustomComboBox");
            //cb.Template = ResourcesManager.Instance.GetControlTemplate("CustomComboBox");
            cb.Padding = new Thickness(0.0);
            cb.Margin = new Thickness(0.0);
            cb.HorizontalAlignment = HorizontalAlignment.Stretch;
            cb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");
            cb.SetBinding(a7ComboBox.SelectedValueProperty, getFilterValueBinding());
            cb.SetBinding(a7ComboBox.IsEnabledProperty, getIsEnabledBinding());
            cb.Height = 18;
            cb.FontSize = 12;
            cb.SelectionChanged += (s, e) => activateFilter();
            return cb;
        }
        
        private Binding getFilterValueBinding()
        {
            return new Binding("Value")
                {
                    Source = this.Filter, 
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, 
                    Mode = BindingMode.TwoWay 
                };
        }

        private Binding getIsEnabledBinding()
        {
            return new Binding("IsEnabled")
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsReadOnlyProperty)
            {
                if (_frameworkElement != null)
                {
                    if (_frameworkElement.PropertyExists("IsReadOnly"))
                        _frameworkElement.SetPropertyValue("IsReadOnly", e.NewValue);
                    else
                        _frameworkElement.IsEnabled = !(bool)(e.NewValue);
                }
            }
        }

        private void activateFilter()
        {
            if (EditorContext != null && EditorContext.UpdateFilterFunction != null && !EditorContext.IsReadOnly)
                EditorContext.UpdateFilterFunction(EditorContext.FilterExpr);
        }
    }
}
