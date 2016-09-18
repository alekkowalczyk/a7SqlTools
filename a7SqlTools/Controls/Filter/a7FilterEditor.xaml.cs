using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using a7SqlTools.TableExplorer;
using a7SqlTools.TableExplorer.Filter;
using a7SqlTools.Utils;

namespace a7SqlTools.Controls.Filter
{
    /// <summary>
    /// Interaction logic for a7FilterEditor.xaml
    /// </summary>
    public partial class a7FilterEditor : UserControl, INotifyPropertyChanged
    {
        public static int BackgroundIndexStep = 18;



        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(a7FilterEditor), new UIPropertyMetadata(false));

        

        public FilterExpressionData FilterExpr
        {
            get { return (FilterExpressionData)GetValue(FilterExprProperty); }
            set { SetValue(FilterExprProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterExpr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterExprProperty =
            DependencyProperty.Register("FilterExpr", typeof(FilterExpressionData), typeof(a7FilterEditor), new UIPropertyMetadata(null));

        

        public Action<FilterExpressionData> UpdateFilterFunction
        {
            get { return (Action<FilterExpressionData>)GetValue(UpdateFilterFunctionProperty); }
            set { SetValue(UpdateFilterFunctionProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RefreshFunction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateFilterFunctionProperty =
            DependencyProperty.Register("UpdateFilterFunction", typeof(Action<FilterExpressionData>), typeof(a7FilterEditor));


        private IEnumerable<a7FilterElementDefinition> _elements;
        public IEnumerable<a7FilterElementDefinition> Elements { get { return _elements; } set { _elements = value; OnPropertyChanged("Elements"); } }
        
        private a7SingleTableExplorer _entity;
        private int _backgroundIndex;
        private a7FilterGroupEditor _rootGroup;
        public List<Popup> EntityFieldsPopups { get; private set; }

        public a7FilterEditor()
        {
            EntityFieldsPopups = new List<Popup>();
            InitializeComponent();
        }

        public void SetTable(a7SingleTableExplorer table)
        {
            if (table.IsNotEmpty())
            {
                _entity = table;
                Elements = a7FilterEditorUtils.GetFilterEditorElements(table);
            }
            Reset();
        }

        public void SetFilter(a7SingleTableExplorer entity, FilterExpressionData filter)
        {
            if (entity == null)
            {
                Reset();
                return;
            }
            Elements = a7FilterEditorUtils.GetFilterEditorElements(entity);
            var fge = new a7FilterGroupEditor(entity, false, this.IsReadOnly, this);
            fge.SetFilter(filter);
            this.FilterExpr = filter;
            SetRootGroup(fge);
            if (filter != null)
            {
                gStartPanel.Visibility = Visibility.Collapsed;
                MyBorder.Visibility = System.Windows.Visibility.Visible;
                if(!IsReadOnly)
                    spButtons.Visibility = System.Windows.Visibility.Visible;
            }
        }



        public void Reset(bool withRefresh = false)
        {
            _backgroundIndex = 0;
            mainGrid.Children.Clear();
            this.FilterExpr = null;
            if (!this.IsReadOnly)
            {
                gStartPanel.Visibility = Visibility.Visible;
                MyBorder.Visibility = Visibility.Collapsed;
                spButtons.Visibility = Visibility.Collapsed;
            }
         //   spButtons.Visibility = Visibility.Collapsed;
            if (withRefresh && UpdateFilterFunction!=null)
                UpdateFilterFunction(null);
        }

        private void fgeOnAddedFirstElement(object sender, EventArgs eventArgs)
        {
            if (!this.IsReadOnly)
            {
                _backgroundIndex += BackgroundIndexStep;
                var newFge = new a7FilterGroupEditor(_entity, true, this.IsReadOnly, this);
                mainGrid.Children.Remove(_rootGroup);
                newFge.AddGroupSubFilter(_rootGroup);
                newFge.SetBackground(_rootGroup.MyBackgroundIndex + BackgroundIndexStep);
                SetRootGroup(newFge);
            }
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.UpdateFilterFunction != null)
                UpdateFilterFunction(FilterExpr);
        }

        private void lbFields_MouseUp(object sender, MouseButtonEventArgs e)
        {
             var selectedField = lbFields.SelectedItem as a7FilterElementDefinition;
             if (selectedField != null)
             {
                 gStartPanel.Visibility = Visibility.Collapsed;
                 MyBorder.Visibility = System.Windows.Visibility.Visible;
                 spButtons.Visibility = System.Windows.Visibility.Visible;
                 var fge = new a7FilterGroupEditor(_entity, false, this.IsReadOnly, this);
                 var fae = new a7FilterElementEditor(selectedField) { Margin = new Thickness(0, 0, 0, 0), IsReadOnly = this.IsReadOnly };
                 fae.EditorContext = this;
                 fge.SetAtomFilter(fae);
                 this.FilterExpr = fge.Filter;
                 SetRootGroup(fge);
             }
        }

        public void SetRootGroup(a7FilterGroupEditor fge)
        {
            if (_rootGroup != null)
            {
                var flt = _rootGroup.Filter; //setvalue, clear binding
                BindingOperations.ClearBinding(_rootGroup, a7FilterGroupEditor.FilterProperty);
                _rootGroup.Filter = flt;
                _rootGroup.AddedFirstElement -= fgeOnAddedFirstElement;
                _rootGroup.Parent = fge;
                mainGrid.Children.Remove(_rootGroup);
            }
            _rootGroup = fge;
            this.SetBinding(a7FilterEditor.FilterExprProperty, new Binding("Filter") { Source = fge, Mode = BindingMode.TwoWay });
      //      fge.Background = Brushes.White;
            fge.AddedFirstElement += fgeOnAddedFirstElement;
            fge.Parent = null;
            mainGrid.Children.Add(fge);
            if(fge.AtomFilter!=null)
                fge.SetAsRoot();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsReadOnlyProperty)
            {
                if (_rootGroup != null)
                    _rootGroup.IsReadOnly = ((bool)e.NewValue);
                if ((bool)e.NewValue)
                {
                    gStartPanel.Visibility = Visibility.Collapsed;
                    MyBorder.Visibility = System.Windows.Visibility.Visible;
                    spButtons.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this._rootGroup != null)
                    {
                        gStartPanel.Visibility = Visibility.Collapsed;
                        MyBorder.Visibility = System.Windows.Visibility.Visible;
                        spButtons.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        gStartPanel.Visibility = Visibility.Visible;
                        MyBorder.Visibility = System.Windows.Visibility.Collapsed;
                        spButtons.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void lbRelations_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
