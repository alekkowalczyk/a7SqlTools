using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using a7SqlTools.TableExplorer;
using a7SqlTools.TableExplorer.Filter;
using a7SqlTools.Utils;

namespace a7SqlTools.Controls.Filter
{
    public class a7FilterEditorButton : Control
    {
        public static readonly DependencyProperty TableProperty =
            DependencyProperty.Register("Table", typeof (a7SingleTableExplorer), typeof (a7FilterEditorButton), new PropertyMetadata(default(a7SingleTableExplorer)));

        public a7SingleTableExplorer Table
        {
            get { return (a7SingleTableExplorer) GetValue(TableProperty); }
            set { SetValue(TableProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(a7FilterEditorButton), new UIPropertyMetadata(false));


        public Brush ActiveBackground
        {
            get { return (Brush)GetValue(ActiveBackgroundProperty); }
            set { SetValue(ActiveBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveBackgroundProperty =
            DependencyProperty.Register("ActiveBackground", typeof(Brush), typeof(a7FilterEditorButton), new UIPropertyMetadata(Brushes.Transparent));

        

        public Action<FilterExpressionData> UpdateFilterFunction
        {
            get { return (Action<FilterExpressionData>)GetValue(UpdateFilterFunctionProperty); }
            set { SetValue(UpdateFilterFunctionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RefreshFunction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateFilterFunctionProperty =
            DependencyProperty.Register("UpdateFilterFunction", typeof(Action<FilterExpressionData>), typeof(a7FilterEditorButton));


        public FilterExpressionData FilterExpr
        {
            get { return (FilterExpressionData)GetValue(FilterExprProperty); }
            set { SetValue(FilterExprProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterExpr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterExprProperty =
            DependencyProperty.Register("FilterExpr", typeof(FilterExpressionData), typeof(a7FilterEditorButton), new UIPropertyMetadata(null));
        

        public a7FilterEditorButton()
        {
            Template = ResourcesManager.Instance.GetControlTemplate("a7FilterEditorButtonTemplate");
        }

        private Popup _fePopup;
        private a7FilterEditor _fePopupControl;
        private a7SingleTableExplorer _table = null;
        private Window myWindow;
        public override void OnApplyTemplate()
        {
            DependencyObject fePopup = GetTemplateChild("fePopup");
            if (fePopup != null)
            {
                _fePopup = fePopup as Popup;
                _fePopup.Opened += new EventHandler(_fePopup_Opened);
                _fePopup.Closed += new EventHandler(_fePopup_Closed);
            }
            var fePopupControl = GetTemplateChild("fePopupControl");
            if (fePopupControl != null)
            {
                _fePopupControl = fePopupControl as a7FilterEditor;
                if(_table!=null)
                    _fePopupControl.SetTable(_table);
                if (FilterExpr != null)
                    _fePopupControl.SetFilter(_table, FilterExpr);
            }
            this.myWindow = Window.GetWindow(this);
            if(myWindow!=null)
                this.myWindow.PreviewMouseDown += new MouseButtonEventHandler(myWindow_PreviewMouseDown);

            if (UpdateFilterFunction != null)
            {
                _fePopupControl.UpdateFilterFunction = UpdateFilter;
            }
            _fePopupControl.SetBinding(a7FilterEditor.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });
        }

        void UpdateFilter(FilterExpressionData filter)
        {
            UpdateFilterFunction?.Invoke(filter);
            if (filter != null && filter.HasActiveFilter)
            {
                this.ActiveBackground = Brushes.Red;
            }
            else
            {
                this.ActiveBackground = Brushes.Transparent;
            }
        }

        void myWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseOver)
                return;
            if (this._fePopupControl != null && this._fePopupControl.IsMouseOver)
                return;
            var isOverPopup = false;
            foreach (Popup pp in _fePopupControl.EntityFieldsPopups)
            {
                if (pp.IsMouseOver)
                    isOverPopup = true;
            }
            if(!isOverPopup)
                this._fePopup.IsOpen = false;
        }




        private void _fePopup_Closed(object sender, EventArgs eventArgs)
        {
            Window wnd = Window.GetWindow(this);
            if (wnd != null)
                wnd.LocationChanged -= wnd_LocationChanged; 

        }

        void _fePopup_Opened(object sender, EventArgs e)
        {
            Window wnd = Window.GetWindow(this);
            if (wnd != null)
                wnd.LocationChanged += new EventHandler(wnd_LocationChanged);
        }

        void wnd_LocationChanged(object sender, EventArgs e)
        {
            var offset = _fePopup.HorizontalOffset;
            _fePopup.HorizontalOffset = offset + 1;
            _fePopup.HorizontalOffset = offset;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == TableProperty && _table != e.NewValue as a7SingleTableExplorer)
            {
                if (_fePopupControl != null)
                {
                    _fePopupControl.SetTable(e.NewValue as a7SingleTableExplorer);
                    _table = e.NewValue as a7SingleTableExplorer;
                    if(this.FilterExpr !=null)
                        _fePopupControl.SetFilter(_table, FilterExpr);
                }
                else
                {
                    _table = e.NewValue as a7SingleTableExplorer;
                }
            }
            else if (e.Property == FilterExprProperty && this.FilterExpr != e.NewValue as FilterExpressionData)
            {
                if (_fePopupControl != null && this._table !=null)
                {
                    _fePopupControl.SetFilter(this._table, e.NewValue as FilterExpressionData);
                }
                else
                {
                    this.FilterExpr = e.NewValue as FilterExpressionData;
                }
            }
            else if (e.Property == UpdateFilterFunctionProperty && this._fePopupControl != null)
            {
                this._fePopupControl.UpdateFilterFunction = UpdateFilter;
            }
        }
    }


}
